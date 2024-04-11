using HikIsapi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                .AddTransient<IIsapi, Isapi>();
            });
        }

        public static IHostBuilder AddKeyedHikIsapi<TConfig>(this IHostBuilder hostBuilder, object key, Func<IServiceProvider, object, TConfig> implementationFactory)
            where TConfig : class, IIsapiConfig
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services
                    .AddKeyedSingleton<IIsapiConfig, TConfig>(key, implementationFactory)
                    .AddSeriRestClient(Isapi.ServiceKey, (sp, _) =>
                    {
                        var config = sp.GetRequiredKeyedService<IIsapiConfig>(key);
                        return new RestClientOptions()
                        {
                            CookieContainer = new CookieContainer(),
                            BaseUrl = new Uri($"{config.Address}/ISAPI"),
                            MaxTimeout = config.MaxTimeout
                        };
                    })
                    .AddKeyedTransient<IIsapi, Isapi>(key, (sp, _) => new Isapi(
                        sp.GetRequiredKeyedService<IIsapiConfig>(key),
                        sp.GetRequiredService<Serilog.ILogger>(),
                        sp.GetRequiredKeyedService<SeriRestClient>(Isapi.ServiceKey)
                        ));
            });
        }
    }
}