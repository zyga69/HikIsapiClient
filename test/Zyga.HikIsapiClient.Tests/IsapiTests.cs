using System.Net;
using Serilog;
using Xunit;
using Zyga.HikIsapiClient.ExtensionMethods;
using Zyga.HikIsapiClient.Tests.TestSupport;
using Zyga.SeriRestClient;

namespace Zyga.HikIsapiClient.Tests;

public class IsapiTests
{
    private static (Isapi Isapi, FakeIsapiServer Server) CreateIsapi(FakeIsapiServer server)
    {
        var config = new IsapiConfig
        {
            Name = "test-device",
            Address = server.BaseUrl.ToString().TrimEnd('/'),
            User = "admin",
            Password = "pass",
            MaxTimeout = 2000
        };

        var client = new LoggingRestClient(config.GetRestClientOptions(), Log.Logger);
        return (new Isapi(config, Log.Logger, client), server);
    }

    private static (int StatusCode, string ContentType, string Body, string? SetCookie) Respond(
        HttpStatusCode status, string body, string contentType = "application/xml", string? setCookie = null)
        => ((int)status, contentType, body, setCookie);

    [Fact]
    public void IsAvailable_WhenCapabilitiesSucceed_ReturnsTrue()
    {
        using var server = new FakeIsapiServer();
        server.Handler = _ => Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities);

        var (isapi, _) = CreateIsapi(server);

        Assert.True(isapi.IsAvailable());
    }

    [Fact]
    public void IsAvailable_WhenCapabilitiesFail_ReturnsFalse()
    {
        using var server = new FakeIsapiServer();
        server.Handler = _ => Respond(HttpStatusCode.Unauthorized, "");

        var (isapi, _) = CreateIsapi(server);

        Assert.False(isapi.IsAvailable());
    }

    [Fact]
    public void Login_WhenCapabilitiesAndLoginSucceed_ReturnsAuthenticatedSession()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);

        var authenticated = isapi.Login();

        Assert.NotNull(authenticated);
        Assert.Equal(0, authenticated.HeartBeatCount);
    }

    [Fact]
    public void Login_WhenCapabilitiesRequestFails_Throws()
    {
        using var server = new FakeIsapiServer();
        server.Handler = _ => Respond(HttpStatusCode.Unauthorized, "");

        var (isapi, _) = CreateIsapi(server);

        Assert.ThrowsAny<Exception>(() => isapi.Login());
    }

    [Fact]
    public void Login_WhenCapabilitiesHaveNoData_ThrowsNoLoginCapabilities()
    {
        using var server = new FakeIsapiServer();
        // 200 with a content type no deserializer understands leaves Data null
        // while IsSuccessful stays true.
        server.Handler = _ => Respond(HttpStatusCode.OK, "ok", contentType: "text/plain");

        var (isapi, _) = CreateIsapi(server);

        var ex = Assert.Throws<Exception>(() => isapi.Login());
        Assert.Equal("No login capabilities", ex.Message);
    }

    [Fact]
    public void EnsureLoggedIn_WhenNotLoggedIn_PerformsLogin()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);

        isapi.EnsureLoggedIn();

        Assert.Equal(2, server.Requests.Count);
    }

    [Fact]
    public void EnsureLoggedIn_WhenAlreadyLoggedIn_DoesNotLoginAgain()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);

        isapi.Login();
        var requestsAfterLogin = server.Requests.Count;

        isapi.EnsureLoggedIn();

        Assert.Equal(requestsAfterLogin, server.Requests.Count);
    }

    [Fact]
    public void Login_WithForce_WhenAlreadyLoggedIn_LogsInAgain()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);

        isapi.Login();
        var requestsAfterFirstLogin = server.Requests.Count;

        isapi.Login(force: true);

        Assert.Equal(requestsAfterFirstLogin * 2, server.Requests.Count);
    }

    [Fact]
    public void SessionHeartBeat_WhenSuccessful_IncrementsHeartBeatCount()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : request.Url!.AbsolutePath.Contains("sessionHeartbeat")
                ? Respond(HttpStatusCode.OK, IsapiXml.ResponseStatusOk)
                : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);
        var authenticated = isapi.Login();

        authenticated.SessionHeartBeat();

        Assert.Equal(1, authenticated.HeartBeatCount);
    }

    [Fact]
    public void SessionHeartBeat_WhenRequestFails_Throws()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : request.Url!.AbsolutePath.Contains("sessionHeartbeat")
                ? Respond(HttpStatusCode.ServiceUnavailable, "")
                : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);
        var authenticated = isapi.Login();

        Assert.ThrowsAny<Exception>(() => authenticated.SessionHeartBeat());
    }

    [Fact]
    public void LogOut_ResetsSessionState()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : request.Url!.AbsolutePath.Contains("sessionLogout")
                ? Respond(HttpStatusCode.OK, IsapiXml.ResponseStatusOk)
                : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);
        var authenticated = isapi.Login();
        var requestsAfterLogin = server.Requests.Count;

        var loggedOut = authenticated.LogOut();
        loggedOut.EnsureLoggedIn();

        // EnsureLoggedIn after LogOut must perform a full login again (capabilities + login),
        // proving the session state was actually reset rather than just returning the same instance.
        Assert.True(server.Requests.Count > requestsAfterLogin + 1);
    }

    [Fact]
    public void DoorOpen_WhenSuccessful_ReturnsAuthenticatedSession()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : request.Url!.AbsolutePath.Contains("door/1")
                ? Respond(HttpStatusCode.OK, IsapiXml.ResponseStatusOk)
                : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);
        var authenticated = isapi.Login();

        var result = authenticated.DoorOpen(1);

        Assert.NotNull(result);
        Assert.Contains(server.Requests, r => r.Path.Contains("door/1"));
    }

    [Fact]
    public void DoorOpen_WhenRequestFails_Throws()
    {
        using var server = new FakeIsapiServer();
        server.Handler = request => request.Url!.AbsolutePath.Contains("capabilities")
            ? Respond(HttpStatusCode.OK, IsapiXml.LoginCapabilities)
            : request.Url!.AbsolutePath.Contains("door/1")
                ? Respond(HttpStatusCode.Forbidden, "")
                : Respond(HttpStatusCode.OK, IsapiXml.LoginSuccess, setCookie: "WebSession=abc123; Path=/");

        var (isapi, _) = CreateIsapi(server);
        var authenticated = isapi.Login();

        Assert.ThrowsAny<Exception>(() => authenticated.DoorOpen(1));
    }
}
