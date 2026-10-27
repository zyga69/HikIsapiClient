using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestSharp;
using System.Net;
using Zyga.SeriRestClient;

namespace Zyga.HikIsapiClient.ExtensionMethods;

/// <summary>
/// Dependency-injection registration helpers for <see cref="IIsapi"/>.
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// Registers a single <see cref="IIsapi"/> instance, bound from the given configuration section.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="configSection">The configuration section to bind an <see cref="IsapiConfig"/> from.</param>
    /// <returns>The same host builder, for chaining.</returns>
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
                 .AddSeriRestClient(Isapi.ServiceKey)
                 .Configure((sp, _, options) => sp.GetRequiredService<IIsapiConfig>()
                    .ApplyRestClientOptions(options))
                 .Services
                 .AddTransient<IIsapi, Isapi>((sp) => new Isapi(
                    sp.GetRequiredService<IIsapiConfig>(),
                    sp.GetRequiredService<Serilog.ILogger>(),
                    sp.GetRequiredKeyedService<LoggingRestClient>($"{Isapi.ServiceKey}")
                    ));
        });
    }

    /// <summary>
    /// Registers a keyed <see cref="IIsapi"/> instance, allowing multiple devices to be
    /// configured independently in the same container.
    /// </summary>
    /// <typeparam name="TConfig">The concrete <see cref="IIsapiConfig"/> implementation to register.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="isapiKey">The service key identifying this device.</param>
    /// <param name="implementationFactory">A factory that produces the device's configuration.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddKeyedHikIsapi<TConfig>(this IServiceCollection services, object isapiKey, Func<IServiceProvider, object?, TConfig> implementationFactory)
        where TConfig : class, IIsapiConfig
    {
        services
            .AddKeyedSingleton<IIsapiConfig, TConfig>(isapiKey, implementationFactory)
            .AddSeriRestClient($"{Isapi.ServiceKey}_{isapiKey}")
            .Configure((sp, _, options) => sp.GetRequiredKeyedService<IIsapiConfig>(isapiKey)
                .ApplyRestClientOptions(options));

        return services
            .AddKeyedScoped<IIsapi, Isapi>(isapiKey, (sp, _) => new Isapi(
                sp.GetRequiredKeyedService<IIsapiConfig>(isapiKey),
                sp.GetRequiredService<Serilog.ILogger>(),
                sp.GetRequiredKeyedService<LoggingRestClient>($"{Isapi.ServiceKey}_{isapiKey}")
                ));
    }

    /// <summary>
    /// Builds the <see cref="RestClientOptions"/> used to talk to a device, from its configuration.
    /// </summary>
    /// <param name="config">The device configuration.</param>
    /// <returns>Configured <see cref="RestClientOptions"/> pointing at the device's ISAPI endpoint.</returns>
    public static RestClientOptions GetRestClientOptions(this IIsapiConfig config)
    {
        var options = new RestClientOptions();
        config.ApplyRestClientOptions(options);
        return options;
    }

    /// <summary>
    /// Applies a device's configuration onto an existing <see cref="RestClientOptions"/> instance.
    /// </summary>
    /// <param name="config">The device configuration.</param>
    /// <param name="options">The options instance to configure.</param>
    private static void ApplyRestClientOptions(this IIsapiConfig config, RestClientOptions options)
    {
        options.CookieContainer = new CookieContainer();
        options.BaseUrl = new Uri($"{config.Address}/ISAPI");
        options.Timeout = TimeSpan.FromMilliseconds(config.MaxTimeout ?? 5000);
    }
}
