using HikIsapi.Client.ExtensionMethods;
using HikIsapi.Client.Model;
using HikIsapi.Client.Model.Request;
using HikIsapi.Client.Model.Response;
using NetDaemonApps.Lib.HikIsapiClient.HikIsapiClient.ExtensionMethods;
using RestSharp;
using ILogger = Serilog.ILogger;

namespace HikIsapi.Client;

public class Isapi : IIsapi, IIsapiAuthenticated
{
    public const string ServiceKey = "ISAPI";

    private readonly SeriRestClient.SeriRestClient client;
    private readonly ILogger logger;
    private readonly IIsapiConfig config;

    private bool loggedIn = false;
    private int heartBeatCount = 0;

    public int HeartBeatCount => heartBeatCount;

    public Isapi(IIsapiConfig config, ILogger logger, SeriRestClient.SeriRestClient seriRestClient)
    {
        this.logger = logger.ForContext($"{ServiceKey}.{config.Name}");
        this.config = config;
        client = seriRestClient;
    }

    private RestResponse<SessionLoginCap> GetLoginCapabilities()
    {
        var request = new RestRequest("Security/sessionLogin/capabilities");
        request.AddQueryParameter("username", config.User);
        request.AddQueryParameter("random", GetRandom());
        request.RequestFormat = DataFormat.Xml;

        logger.Information($"Getting Login Capabilities for user [{config.User}]");

        return client.LogRequest<SessionLoginCap>(request, Method.Get);
    }

    public bool IsAvailable() => GetLoginCapabilities().IsSuccessful;

    public IIsapiAuthenticated Login()
    {
        var loginCapabilities = GetLoginCapabilities();
        loginCapabilities.ThrowIfError();

        if (loginCapabilities.Data == null)
            throw new Exception("No login capabilities");

        Login(loginCapabilities.Data);

        return this;
    }

    public IIsapiAuthenticated EnsureLoggedIn()
    {
        if (loggedIn) 
            return this;

        return Login();
    }

    public IIsapiAuthenticated Login(bool force = false)
    {
        if (force) loggedIn = false;

        if (loggedIn) return this;

        var loginCapabilities = GetLoginCapabilities();
        loginCapabilities.ThrowIfError();

        if (loginCapabilities.Data == null)
            throw new Exception("No login capabilities");

        Login(loginCapabilities.Data);

        return this;
    }

    private void Login(SessionLoginCap capabilitiesResponse)
    {
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
            logger.Error("No cookies in login response");
            throw new Exception("No cookies in login response");
        }

        client.Options?.CookieContainer?.Add(response.Cookies);

        loggedIn = true;
        heartBeatCount = 0;

        logger.Information($"Successfully Logged-In as [{config.User}]");
    }

    public IIsapiAuthenticated SessionHeartBeat()
    {
        var hearBeatRequest = new RestRequest("Security/sessionHeartbeat");
        var response = client.LogRequest<Model.Response.ResponseStatus>(hearBeatRequest, Method.Put);
        response.ThrowIfError();

        if (!(response.IsSuccessful && response.Data?.StatusCode == (int)ResponseStatusString.OK))
            throw new Exception($"Unable to execute heartbeat!");

        heartBeatCount++;
        logger.Information($"Session heartbeat sent");

        return this;
    }

    public IIsapi LogOut()
    {
        var logoutRequest = new RestRequest("Security/sessionLogout");
        client.LogRequest<Model.Response.ResponseStatus>(logoutRequest, Method.Put);

        loggedIn = false;
        logger.Information($"Successfully Logged-Out");
        return this;
    }

    public IIsapiAuthenticated DoorOpen(int doorNo)
    {
        var request = new RestRequest($"AccessControl/RemoteControl/door/{doorNo}")
        {
            RequestFormat = DataFormat.Xml
        };
        request.AddBody(new RemoteControlDoor());

        var response = client.LogRequest<Model.Response.ResponseStatus>(request, Method.Put);
        response.ThrowIfError();

        if (!(response.IsSuccessful && response.Data?.StatusCode == (int)ResponseStatusString.OK))
            throw new Exception($"Unable to open door!");

        logger.Information($"Door [{doorNo}] successfully opened");

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