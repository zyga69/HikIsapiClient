

using HikIsapi.Client;
using HikIsapi.Client.Model;

namespace HikIsapi.Test;

internal class Program
{
    private static void Main(string[] args)
    {
        using (var isapi = new Isapi(new Config())) { }


    }
}

public class Config : IIsapiConfig
{
    public string Address => "http://192.168.0.163";

    public string User => "admin";

    public string Password => "REDACTED";
}
