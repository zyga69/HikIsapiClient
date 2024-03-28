
using System.Net;
using HikIsapi.Client.ExtensionMethods;
using HikIsapi.Client.Model;
using HikIsapi.Client.Model.Request;
using HikIsapi.Client.Model.Response;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace HikIsapi.Client;

public class Isapi : IIsapi
{
    private readonly RestClient client;
    private readonly Logger logger;
    private readonly CookieContainer cookies = new();

    public Isapi(IIsapiConfig config)
    {
        logger = new LoggerConfiguration()
             .WriteTo.Console(theme: AnsiConsoleTheme.Literate, applyThemeToRedirectedOutput: true)
             .MinimumLevel.Debug()
             .CreateLogger();

        client = new RestClient($"{config.Address}/ISAPI");

        var loginCapabilities = GetLoginCapbilities(config.User);
        if (loginCapabilities != null)
            Login(config.User, config.Password, loginCapabilities);

    }

    private SessionLoginCap? GetLoginCapbilities(string user)
    {
        var capabilitiesRequest = new RestRequest("Security/sessionLogin/capabilities");
        capabilitiesRequest.AddQueryParameter("username", user);
        capabilitiesRequest.AddQueryParameter("random", GetRandom());

        logger.Debug($"Executing [{capabilitiesRequest.Method}] -> [{capabilitiesRequest.Resource}]");



        logger.Debug($"Getting Login Capabilities for user [{user}]");

        var response = client.ExecuteGet<SessionLoginCap>(capabilitiesRequest);

        if (!response.IsSuccessful)
        {
            logger.Error("Could not GET Login Capabilities");
            logger.Error($"[{response.StatusCode}]: [{response.ErrorMessage}]");
            return null;
        }

        logger.Debug(JsonConvert.SerializeObject(response.Data));
        return response.Data;
    }

    private void Login(string user, string password, SessionLoginCap capabilitiesResponse)
    {

        logger.Debug($"Trying to login");
        logger.Debug($" Login Capabilities -> Challenge:      [{capabilitiesResponse.Challenge}]");
        logger.Debug($" Login Capabilities -> Salt:           [{capabilitiesResponse.Salt}]");
        logger.Debug($" Login Capabilities -> Iterations:     [{capabilitiesResponse.Iterations}]");
        logger.Debug($" Login Capabilities -> IsIrreversible: [{capabilitiesResponse.IsIrreversible}]");
        logger.Debug($" Login Capabilities -> SessionID:      [{capabilitiesResponse.SessionID}]");

        var passwordHash = password.ToPasswordHash(
            capabilitiesResponse.Challenge,
            user,
            capabilitiesResponse.Salt,
            capabilitiesResponse.Iterations,
            capabilitiesResponse.IsIrreversible);

        logger.Debug($" Passwordhash: [{passwordHash}]");

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

        var response = client.ExecutePost(loginRequest);

        if (!response.IsSuccessful)
        {
            logger.Error("Could not Login");
            logger.Error($"[{response.StatusCode}]: [{response.ErrorMessage}]");
            return;
        }

        if (response.Cookies == null)
        {
            logger.Error("No cookies in login response");
            return;
        }


        foreach (Cookie cookie in response.Cookies)
        {
            logger.Debug($" Cookie -> {cookie.Name}: [{cookie.Value}]");
        }

        cookies.Add(response.Cookies);
    }

    private void LogOut()
    {
        var logoutRequest = new RestRequest("Security/sessionLogout")
        {
            CookieContainer = cookies
        };
        var response = client.ExecutePut(logoutRequest);

        if (!response.IsSuccessful)
        {
            logger.Error("Could not Logout");
            logger.Error($"[{response.StatusCode}]: [{response.ErrorMessage}]");
        }
        else
        {
            logger.Debug("Succesfully Logged Out");
        }
    }

    public bool DoorOpen(int doorNo)
    {
        var request = new RestRequest($"AccessControl/RemoteControl/door/{doorNo}")
        {
            RequestFormat = DataFormat.Xml,
            CookieContainer = cookies
        };
        request.AddBody(new RemoteControlDoor());

        var response = client.ExecutePut<Model.Response.ResponseStatus>(request);
        if (!response.IsSuccessful)
        {
            logger.Error("Could not Open Door");
            logger.Error($"[{response.StatusCode}]: [{response.ErrorMessage}]");
            return false;
        }

        if (response.Data?.StatusCode != (int)ResponseStatusString.OK)
        {
            logger.Error("Could not Open Door");
            logger.Error($"[{response.Data?.StatusCode}]: [{response.Data?.StatusString}] - [{response.Data?.SubStatusCode}]");
            return false;
        }

        return true;


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