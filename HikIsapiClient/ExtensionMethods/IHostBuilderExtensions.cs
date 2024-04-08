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
        public static IHostBuilder AddHikIsapi(this IHostBuilder hostBuilder, string configSection = "Isapi") //where TConfig : class, IIsapiConfig
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
                        .AddScoped<IIsapi, Isapi>();
            });
        }
    }
}