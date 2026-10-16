namespace Zyga.HikIsapiClient;

/// <summary>
/// Default <see cref="IIsapiConfig"/> implementation, suitable for binding from
/// <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
/// </summary>
public class IsapiConfig : IIsapiConfig
{
    /// <inheritdoc/>
    public required string Name { get; set; }

    /// <inheritdoc/>
    public required string Address { get; set; }

    /// <inheritdoc/>
    public required string User { get; set; }

    /// <inheritdoc/>
    public required string Password { get; set; }

    /// <inheritdoc/>
    public int? MaxTimeout { get; set; }
}
