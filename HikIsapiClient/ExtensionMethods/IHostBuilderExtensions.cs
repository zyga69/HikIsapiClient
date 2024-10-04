using HikIsapi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemonApps.apps.Function;
using RestSharp;
using SeriRest;
using System.Net;

namespace hikisapiclient.ExtensionMethods
{
    public static class IHostBuilderExtensions
    {
        public static IHostBuilder AddHikIsapi(this IHostBuilder hostBuilder, string configSection)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services
                    .AddSingleton<IIsapiConfig>((sp) => sp
                            .GetRequiredService<IConfiguration>()
                            .GetRequiredSection(configSection)
                            .Get<IsapiConfig>() ?? throw new Exception($"Section [{configSection}] not found in config")
                        )
                     .AddSeriRestClient(Isapi.ServiceKey, (sp, key) =>
                     {
                         var config = sp.GetRequiredService<IIsapiConfig>();
                         return new RestClientOptions()
                         {
                             CookieContainer = new CookieContainer(),
                             BaseUrl = new Uri($"{config.Address}/ISAPI"),
                             MaxTimeout = config.MaxTimeout
                         };
                     })
                     .AddTransient<IIsapi, Isapi>((sp) => new Isapi(
                        sp.GetRequiredService<IIsapiConfig>(),
                        sp.GetRequiredService<Serilog.ILogger>(),
                        sp.GetRequiredKeyedService<SeriRestClient>($"{Isapi.ServiceKey}")
                        ));
            });
        }

        public static IHostBuilder AddKeyedHikIsapi<TConfig>(this IHostBuilder hostBuilder, object isapiKey, Func<IServiceProvider, object, TConfig> implementationFactory)
            where TConfig : class, IIsapiConfig
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services
                    .AddKeyedSingleton<IIsapiConfig, TConfig>(isapiKey, implementationFactory)
                    .AddSeriRestClient($"{Isapi.ServiceKey}_{isapiKey}", (sp, seriRestKey) =>
                    {
                        var config = sp.GetRequiredKeyedService<IIsapiConfig>(isapiKey);
                        return new RestClientOptions()
                        {
                            CookieContainer = new CookieContainer(),
                            BaseUrl = new Uri($"{config.Address}/ISAPI"),
                            Timeout = TimeSpan.FromMilliseconds(config.MaxTimeout)
                        };
                    })
                    .AddKeyedScoped<IIsapi, Isapi>(isapiKey, (sp, _) => new Isapi(
                        sp.GetRequiredKeyedService<IIsapiConfig>(isapiKey),
                        sp.GetRequiredService<Serilog.ILogger>(),
                        sp.GetRequiredKeyedService<SeriRestClient>($"{Isapi.ServiceKey}_{isapiKey}")
                        ));
            });
        }
    }
}