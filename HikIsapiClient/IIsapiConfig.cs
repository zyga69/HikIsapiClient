namespace HikIsapi.Client;

public interface IIsapiConfig
{
    string Address { get; }
    string User { get; }
    string Password { get; }
    int MaxTimeout { get; }
}