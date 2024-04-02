
using System.Diagnostics;
using System.Net;
using HikIsapi.Client.ExtensionMethods;
using HikIsapi.Client.Model;
using HikIsapi.Client.Model.Request;
using HikIsapi.Client.Model.Response;
using hikisapiclient.SeriRestClient;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace HikIsapi.Client;

public class Isapi : IIsapi
{
    private readonly SeriRestClient client;
    private readonly Logger logger;
    private readonly CookieContainer cookies = new();

    public Isapi(IIsapiConfig config)
    {
        logger = new LoggerConfiguration()
             .WriteTo.Console(theme: SystemConsoleTheme.Colored, applyThemeToRedirectedOutput: true)
             .MinimumLevel.Debug()
             .CreateLogger();

        var options = new RestClientOptions()
        {
            BaseUrl = new Uri($"{config.Address}/ISAPI"),
            CookieContainer = cookies
        };

        client = new SeriRestClient(options, logger);

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


        logger.Information($"Getting Login Capabilities for user [{user}]");

        var response = client.LogGet<SessionLoginCap>(request);

        return response.Data;
    }

    private void Login(string user, string password, SessionLoginCap capabilitiesResponse)
    {

        logger.Information($"Trying to login");
        
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

        var response = client.LogPost<SessionLogin>(loginRequest);

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
        var logoutRequest = new RestRequest("Security/sessionLogout");
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
            RequestFormat = DataFormat.Xml
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

        LogRequest(request, response);
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

    private void LogRequest(RestRequest request, RestResponse response)
    {
        var requestToLog = new
        {
            resource = request.Resource,
            // Parameters are custom anonymous objects in order to have the parameter type as a nice string
            // otherwise it will just show the enum value
            parameters = request.Parameters.Select(parameter => new
            {
                name = parameter.Name,
                value = parameter.Value,
                type = parameter.Type.ToString()
            }),
            // ToString() here to have the method as a nice string otherwise it will just show the enum value
            method = request.Method.ToString(),
            // This will generate the actual Uri used in the request
            uri = client.BuildUri(request),
        };

        var responseToLog = new
        {
            statusCode = response.StatusCode,
            content = response.Content,
            headers = response.Headers,
            // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
            responseUri = response.ResponseUri,
            errorMessage = response.ErrorMessage,
        };

        logger.Debug($"Request: {JsonConvert.SerializeObject(requestToLog)}, Response: {JsonConvert.SerializeObject(responseToLog)}");
    }
}