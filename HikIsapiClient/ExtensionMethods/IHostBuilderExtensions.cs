using HikIsapi.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestSharp;

namespace hikisapiclient.ExtensionMethods
{
    public static class IHostBuilderExtensions
    {
        public static IHostBuilder AddHikIsapi<TConfig>(this IHostBuilder hostBuilder) where TConfig : class, IIsapiConfig
        {
            return hostBuilder.ConfigureServices((context, services) =>
            { 
                services.AddSingleton<IIsapiConfig, TConfig>();
                services.AddSingleton((sp) => new RestClientOptions()
                {
                    CookieContainer = new System.Net.CookieContainer(),
                    BaseUrl = new Uri($"{sp.GetRequiredService<IIsapiConfig>().Address}/ISAPI")
                });
                services.AddScoped<SeriRestClient.SeriRestClient>();
                services.AddScoped<IIsapi, Isapi>();
            });
        }
    }
}