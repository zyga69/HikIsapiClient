namespace HikIsapi.Client;

public interface IIsapiConfig
{
    string Name { get; set; }
    string Address { get; }
    string User { get; }
    string Password { get; }

    /// <summary>
    /// Maximum request timeout in milliseconds
    /// </summary>
    int? MaxTimeout { get; }
}