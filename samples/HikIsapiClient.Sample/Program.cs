using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Zyga.HikIsapiClient;
using Zyga.HikIsapiClient.ExtensionMethods;

namespace HikIsapiClient.Sample;

internal class Program
{
    private static void Main(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .AddHikIsapi("Isapi")
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored, applyThemeToRedirectedOutput: true));

        using (IHost host = hostBuilder.Build())
        {
            var api = host.Services.GetRequiredService<IIsapi>();

            if (api.IsAvailable())
            {
                api.Login()
                    .SessionHeartBeat()
                    //.DoorOpen(1)
                    .SessionHeartBeat()
                    .LogOut();
            }
        }
    }
}
