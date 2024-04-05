using HikIsapi.Client.ExtensionMethods;
using HikIsapi.Client.Model;
using HikIsapi.Client.Model.Request;
using HikIsapi.Client.Model.Response;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using Serilog;
using SeriRest;

namespace HikIsapi.Client;

public class Isapi : IIsapi
{
    public const string ServiceKey = "ISAPI";

    private readonly SeriRestClient client;
    private readonly ILogger logger;

    public Isapi(IIsapiConfig config, ILogger logger, [FromKeyedServices(ServiceKey)] SeriRestClient seriRestClient)
    {
        this.logger = logger;
        client = seriRestClient;

        var loginCapabilities = GetLoginCapbilities(config.User);
        if (loginCapabilities != null)
            Login(config.User, config.Password, loginCapabilities);
    }

    private SessionLoginCap? GetLoginCapbilities(string user)
    {
        var request = new RestRequest("Security/sessionLogin/capabilities");
        request.AddQueryParameter("username", user);
        request.AddQueryParameter("random", GetRandom());
        request.RequestFormat = DataFormat.Xml;

        logger.Information($"[ISAPI] Getting Login Capabilities for user [{user}]");

        var response = client.LogRequest<SessionLoginCap>(request, Method.Get);
        response.ThrowIfError();

        return response.Data;
    }

    private void Login(string user, string password, SessionLoginCap capabilitiesResponse)
    {
        logger.Information($"[ISAPI] Log in");

        var passwordHash = password.ToPasswordHash(
            capabilitiesResponse.Challenge,
            user,
            capabilitiesResponse.Salt,
            capabilitiesResponse.Iterations,
            capabilitiesResponse.IsIrreversible);

        var loginRequest = new RestRequest("Security/sessionLogin")
        {
            RequestFormat = DataFormat.Xml
        };
        loginRequest.AddQueryParameter("timeStamp", GetTime());

        loginRequest.AddBody(new SessionLogin()
        {
            UserName = user,
            Password = passwordHash,
            SessionID = capabilitiesResponse.SessionID,
            IsSupportSessionTag = false,
            IsSessionIDValidLongTerm = false,
            SessionIDVersion = 2
        });

        var response = client.LogRequest<SessionLogin>(loginRequest, Method.Post);
        response.ThrowIfError();

        if (response.Cookies == null)
        {
            logger.Error("[ISAPI] No cookies in login response");
            return;
        }

        client.Options?.CookieContainer?.Add(response.Cookies);
    }

    private void LogOut()
    {
        logger.Information($"[ISAPI] Log out");
        var logoutRequest = new RestRequest("Security/sessionLogout");
        client.LogRequest<Model.Response.ResponseStatus>(logoutRequest, Method.Put);
    }

    public bool DoorOpen(int doorNo)
    {
        logger.Information($"[ISAPI] Open Door [{doorNo}]");

        var request = new RestRequest($"AccessControl/RemoteControl/door/{doorNo}")
        {
            RequestFormat = DataFormat.Xml
        };
        request.AddBody(new RemoteControlDoor());

        var response = client.LogRequest<Model.Response.ResponseStatus>(request, Method.Put);
        response.ThrowIfError();

        return (response.IsSuccessful && response.Data?.StatusCode == (int)ResponseStatusString.OK);
    }

    public void Dispose()
    {
        LogOut();
    }

    private static int GetRandom()
    {
        return new Random().Next(10000000, 99999999);
    }

    private static long GetTime()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(utcNow - epoch).TotalMilliseconds;
    }
}