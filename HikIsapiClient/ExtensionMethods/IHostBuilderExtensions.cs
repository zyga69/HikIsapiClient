using HikIsapi.Client;
using hikisapiclient.SeriRestClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestSharp;
using Serilog;
using System.Net;

namespace hikisapiclient.ExtensionMethods
{
    public static class IHostBuilderExtensions
    {
        public static IHostBuilder AddHikIsapi<TConfig>(this IHostBuilder hostBuilder) where TConfig : class, IIsapiConfig
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services
                    .AddSingleton<IIsapiConfig, TConfig>()
                    .AddSeriRestClient(Isapi.ServiceKey, (sp, key) => new RestClientOptions()
                    {
                        CookieContainer = new CookieContainer(),
                        BaseUrl = new Uri($"{sp.GetRequiredService<IIsapiConfig>().Address}/ISAPI")
                    })
                    .AddScoped<IIsapi, Isapi>();
            });
        }
    }
}