using HikIsapi.Client;
using hikisapiclient.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace HikIsapiTest;

internal class Program
{
    private static void Main(string[] args)
    {
        using (IHost host = GetHostBuilder().Build())
        {
            host.Services.GetRequiredService<IIsapi>().DoorOpen(1);
        }
    }

    private static IHostBuilder GetHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<App>();
            })
            .AddHikIsapi<Config>()
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored, applyThemeToRedirectedOutput: true));
    }
}

public class Config : IIsapiConfig
{
    public string Address => "http://192.168.0.163";

    public string User => "admin";

    public string Password => "REDACTED";
}