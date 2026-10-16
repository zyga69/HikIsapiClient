namespace Zyga.HikIsapiClient;

/// <summary>
/// Represents the configuration settings required to connect to an ISAPI service.
/// </summary>
public interface IIsapiConfig
{
    /// <summary>
    /// Gets or sets the configuration name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the address of the ISAPI service.
    /// </summary>
    string Address { get; }

    /// <summary>
    /// Gets the user name for authentication.
    /// </summary>
    string User { get; }

    /// <summary>
    /// Gets the password for authentication.
    /// </summary>
    string Password { get; }

    /// <summary>
    /// Maximum request timeout in milliseconds.
    /// </summary>
    int? MaxTimeout { get; }
}
