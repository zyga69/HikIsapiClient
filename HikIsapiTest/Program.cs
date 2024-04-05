using HikIsapi.Client;
using hikisapiclient;
using hikisapiclient.ExtensionMethods;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace HikIsapiTest;

internal class Program
{
    private static void Main(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .AddHikIsapi()
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored, applyThemeToRedirectedOutput: true));

        using (IHost host = hostBuilder.Build())
        {
            host.Services.GetRequiredService<IIsapi>()
                .Login()
                .DoorOpen(1)
                .LogOut();
        }
    }
}

