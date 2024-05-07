using HikIsapi.Client.ExtensionMethods;
using HikIsapi.Client.Model;
using HikIsapi.Client.Model.Request;
using HikIsapi.Client.Model.Response;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using SeriRest;
using ILogger = Serilog.ILogger;

namespace HikIsapi.Client;

public class Isapi : IIsapi, IIsapiAuthenticated
{
    public const string ServiceKey = "ISAPI";

    private readonly SeriRestClient client;
    private readonly ILogger logger;
    private readonly IIsapiConfig config;

    public Isapi( IIsapiConfig config, ILogger logger,SeriRestClient seriRestClient)
    {
        this.logger = logger;
        this.config = config;
        client = seriRestClient;
    }

    private RestResponse<SessionLoginCap> GetLoginCapbilities()
    {
        var request = new RestRequest("Security/sessionLogin/capabilities");
        request.AddQueryParameter("username", config.User);
        request.AddQueryParameter("random", GetRandom());
        request.RequestFormat = DataFormat.Xml;

        logger.Information($"[ISAPI] Getting Login Capabilities for user [{config.User}]");

        return client.LogRequest<SessionLoginCap>(request, Method.Get);
    }

    public bool IsAvailable() => GetLoginCapbilities().IsSuccessful;

    public IIsapiAuthenticated Login()
    {
        var loginCapabilities = GetLoginCapbilities();
        loginCapabilities.ThrowIfError();

        if (loginCapabilities.Data == null)
            throw new Exception("No login capabilities");

        Login(loginCapabilities.Data);

        return this;
    }

    private void Login(SessionLoginCap capabilitiesResponse)
    {
        logger.Information($"[ISAPI] Log in");

        var passwordHash = config.Password.ToPasswordHash(
            capabilitiesResponse.Challenge,
            config.User,
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
            UserName = config.User,
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
            throw new Exception("[ISAPI] No cookies in login response");
        }

        client.Options?.CookieContainer?.Add(response.Cookies);
    }

    public IIsapi LogOut()
    {
        logger.Information($"[ISAPI] Log out");
        var logoutRequest = new RestRequest("Security/sessionLogout");
        client.LogRequest<Model.Response.ResponseStatus>(logoutRequest, Method.Put);

        return this;
    }

    public IIsapiAuthenticated DoorOpen(int doorNo)
    {
        logger.Information($"[ISAPI] Open Door [{doorNo}]");

        var request = new RestRequest($"AccessControl/RemoteControl/door/{doorNo}")
        {
            RequestFormat = DataFormat.Xml
        };
        request.AddBody(new RemoteControlDoor());

        var response = client.LogRequest<Model.Response.ResponseStatus>(request, Method.Put);
        response.ThrowIfError();

        if (!(response.IsSuccessful && response.Data?.StatusCode == (int)ResponseStatusString.OK))
            throw new Exception($"Unable to open door!");

        return this;
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