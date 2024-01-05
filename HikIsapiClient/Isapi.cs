
using System.Runtime.Serialization;
using HikIsapi.Client.ExtensionMethods;
using HikIsapi.Client.Model;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace HikIsapi.Client;

public class Isapi : IIsapi
{
    private readonly RestClient client;
    private readonly ILogger<Isapi> logger;

    public Isapi(IIsapiConfig config)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        logger = loggerFactory.CreateLogger<Isapi>();

        client = new RestClient(new RestClientOptions()
        {
            CookieContainer = new System.Net.CookieContainer(),
            BaseUrl = new Uri($"{config.Address}/ISAPI")
        });

        Login(config.User, config.Password, GetLoginCapbilities(config.User));
    }

    private SessionLoginCap GetLoginCapbilities(string user)
    {
        var capabilitiesRequest = new RestRequest("Security/sessionLogin/capabilities");
        capabilitiesRequest.AddQueryParameter("username", user);
        capabilitiesRequest.AddQueryParameter("random", GetRandom());

        return client.Get<SessionLoginCap>(capabilitiesRequest) ?? throw new Exception("Could not load login capabilities.");
    }

    private void Login(string user, string password, SessionLoginCap capabilitiesResponse)
    {

        var passwordHash = password.ToPasswordHash(
            capabilitiesResponse.Challenge,
            user,
            capabilitiesResponse.Salt,
            capabilitiesResponse.Iterations,
            capabilitiesResponse.IsIrreversible);
        logger.LogDebug($"Passwordhash: {passwordHash}");
    }






    public Model.ResponseStatus DoorOpen(int doorNo)
    {
        throw new NotImplementedException();
    }

    private static int GetRandom()
    {
        return new Random().Next(10000000, 99999999);
    }

    Model.ResponseStatus IIsapi.DoorOpen(int doorNo)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        logger.LogInformation("Logged Out");
        //client.Put(new RestRequest("Security/sessionLogout"));
    }
}