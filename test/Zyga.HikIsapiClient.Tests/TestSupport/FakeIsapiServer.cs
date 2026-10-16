using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Zyga.HikIsapiClient.Tests.TestSupport;

/// <summary>
/// A minimal in-process HTTP server that stands in for a Hikvision device, so
/// <see cref="Isapi"/> can be exercised end-to-end (including XML deserialization
/// and cookie handling) without a real device on the network.
/// </summary>
internal sealed class FakeIsapiServer : IDisposable
{
    private readonly HttpListener listener;
    private readonly CancellationTokenSource cts = new();
    private readonly Task listenTask;

    public Uri BaseUrl { get; }

    public List<(string Method, string Path)> Requests { get; } = new();

    public Func<HttpListenerRequest, (int StatusCode, string ContentType, string Body, string? SetCookie)> Handler { get; set; }
        = _ => (404, "text/plain", "not found", null);

    public FakeIsapiServer()
    {
        var port = GetFreePort();
        BaseUrl = new Uri($"http://127.0.0.1:{port}/");

        listener = new HttpListener();
        listener.Prefixes.Add(BaseUrl.ToString());
        listener.Start();

        listenTask = Task.Run(() => ListenAsync(cts.Token));
    }

    private async Task ListenAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return;
            }

            lock (Requests)
            {
                Requests.Add((context.Request.HttpMethod, context.Request.Url!.AbsolutePath));
            }

            var (statusCode, contentType, body, setCookie) = Handler(context.Request);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = contentType;
            if (setCookie != null)
            {
                context.Response.Headers.Add("Set-Cookie", setCookie);
            }

            var bytes = Encoding.UTF8.GetBytes(body);
            context.Response.ContentLength64 = bytes.Length;
            await context.Response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
            context.Response.OutputStream.Close();
        }
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void Dispose()
    {
        cts.Cancel();
        listener.Stop();
        listener.Close();
    }
}
