using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Zyga.HikIsapiClient.ExtensionMethods;

namespace Zyga.HikIsapiClient.Tests;

public class RegistrationExtensionsTests
{
    private sealed class StubConfig : IIsapiConfig
    {
        public required string Name { get; set; }
        public string Address { get; set; } = "http://192.168.1.100";
        public string User { get; set; } = "admin";
        public string Password { get; set; } = "unused";
        public int? MaxTimeout { get; set; }
    }

    [Fact]
    public void AddKeyedHikIsapi_RegistersResolvableIIsapi()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Log.Logger);

        services.AddKeyedHikIsapi<StubConfig>("device-a", (_, key) => new StubConfig { Name = $"{key}" });

        using var provider = services.BuildServiceProvider();
        var isapi = provider.GetRequiredKeyedService<IIsapi>("device-a");

        Assert.NotNull(isapi);
    }

    [Fact]
    public void GetRestClientOptions_BuildsIsapiBaseUrl()
    {
        var config = new StubConfig { Name = "device-a", Address = "http://192.168.1.100" };

        var options = config.GetRestClientOptions();

        Assert.Equal(new Uri("http://192.168.1.100/ISAPI"), options.BaseUrl);
    }
}
