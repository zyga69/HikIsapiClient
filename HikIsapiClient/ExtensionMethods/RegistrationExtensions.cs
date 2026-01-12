using HikIsapi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestSharp;
using SeriRestClient;
using System.Net;

namespace hikisapiclient.ExtensionMethods
{
    public static class RegistrationExtensions
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
                         return sp.GetRequiredService<IIsapiConfig>()
                            .GetRestClientOptions();
                     })
                     .AddTransient<IIsapi, Isapi>((sp) => new Isapi(
                        sp.GetRequiredService<IIsapiConfig>(),
                        sp.GetRequiredService<Serilog.ILogger>(),
                        sp.GetRequiredKeyedService<SeriRestClient.SeriRestClient>($"{Isapi.ServiceKey}")
                        ));
            });
        }

        public static IServiceCollection AddKeyedHikIsapi<TConfig>(this IServiceCollection services, object isapiKey, Func<IServiceProvider, object?, TConfig> implementationFactory)
            where TConfig : class, IIsapiConfig
        {
            return services
                .AddKeyedSingleton<IIsapiConfig, TConfig>(isapiKey, implementationFactory)
                .AddSeriRestClient($"{Isapi.ServiceKey}_{isapiKey}", (sp, seriRestKey) =>
                {
                    return sp.GetRequiredKeyedService<IIsapiConfig>(isapiKey)
                        .GetRestClientOptions();
                })
                .AddKeyedScoped<IIsapi, Isapi>(isapiKey, (sp, _) => new Isapi(
                    sp.GetRequiredKeyedService<IIsapiConfig>(isapiKey),
                    sp.GetRequiredService<Serilog.ILogger>(),
                    sp.GetRequiredKeyedService<SeriRestClient.SeriRestClient>($"{Isapi.ServiceKey}_{isapiKey}")
                    ));
        }

        public static RestClientOptions GetRestClientOptions(this IIsapiConfig config)
        {
            return new RestClientOptions()
            {
                CookieContainer = new CookieContainer(),
                BaseUrl = new Uri($"{config.Address}/ISAPI"),
                Timeout = TimeSpan.FromMilliseconds(config.MaxTimeout ?? 5000)
            };
        }
    }
}