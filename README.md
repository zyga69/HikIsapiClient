# HikIsapiClient

A .NET client library for interacting with Hikvision devices using the ISAPI protocol, with integrated Serilog logging capabilities.

## Features

- 🔐 **Session Management** - Automatic login/logout handling with session keep-alive
- 🚪 **Door Control** - Remote door control operations
- 📝 **Logging** - Built-in Serilog integration for comprehensive request/response logging
- 🔄 **State Management** - Smart session state handling with `EnsureLoggedIn()`
- 🔌 **Dependency Injection** - First-class support for Microsoft.Extensions.DependencyInjection
- 🎯 **Keyed Services** - Support for multiple Hikvision device instances
- ⚙️ **Configuration** - Easy configuration through IConfiguration

## Installation

Install via NuGet:

```bash
dotnet add package HikIsapiClient
```

## Usage

### Basic Setup

Configure your Hikvision device in `appsettings.json`:

```json
{
  "HikDevice": {
    "Name": "Main Entrance",
    "Address": "http://192.168.1.100",
    "User": "admin",
    "Password": "your-password",
    "MaxTimeout": 5000
  }
}
```

Register in your `Program.cs` or `Startup.cs`:

```csharp
using hikisapiclient.ExtensionMethods;

Host.CreateDefaultBuilder(args)
    .AddHikIsapi("HikDevice")
    .Build()
    .Run();
```

### Using the Client

```csharp
public class MyService
{
    private readonly IIsapi _isapi;
    
    public MyService(IIsapi isapi)
    {
        _isapi = isapi;
    }
    
    public void OpenDoor()
    {
        // Check if device is available
        if (!_isapi.IsAvailable())
        {
            throw new Exception("Device is not available");
        }
        
        // Login and open door #1
        _isapi.Login()
              .DoorOpen(1)
              .LogOut();
    }
    
    public void MaintainSession()
    {
        // Ensure logged in (will login only if needed)
        var authenticated = _isapi.EnsureLoggedIn();
        
        // Send heartbeat to keep session alive
        authenticated.SessionHeartBeat();
        
        // Check heartbeat count
        Console.WriteLine($"Heartbeats sent: {authenticated.HeartBeatCount}");
    }
}
```

### Multiple Devices (Keyed Services)

Register multiple Hikvision devices:

```csharp
services
    .AddKeyedHikIsapi("entrance", (sp, key) => 
        sp.GetRequiredService<IConfiguration>()
          .GetSection("Devices:Entrance")
          .Get<IsapiConfig>())
    .AddKeyedHikIsapi("parking", (sp, key) => 
        sp.GetRequiredService<IConfiguration>()
          .GetSection("Devices:Parking")
          .Get<IsapiConfig>());
```

Use keyed services:

```csharp
public class SecurityService
{
    private readonly IIsapi _entranceDevice;
    private readonly IIsapi _parkingDevice;
    
    public SecurityService(
        [FromKeyedServices("entrance")] IIsapi entranceDevice,
        [FromKeyedServices("parking")] IIsapi parkingDevice)
    {
        _entranceDevice = entranceDevice;
        _parkingDevice = parkingDevice;
    }
}
```

## API Reference

### IIsapi Interface

| Method | Description |
|--------|-------------|
| `Login()` | Authenticate and establish a session with the device |
| `EnsureLoggedIn()` | Login only if not already authenticated |
| `IsAvailable()` | Check if the device is reachable |

### IIsapiAuthenticated Interface

| Method | Description |
|--------|-------------|
| `DoorOpen(int doorNo)` | Trigger door opening for specified door number |
| `SessionHeartBeat()` | Send keep-alive signal to maintain session |
| `LogOut()` | End the current session |
| `HeartBeatCount` | Get the number of heartbeats sent in current session |

### IIsapiConfig Interface

| Property | Description |
|----------|-------------|
| `Name` | Friendly name for the device |
| `Address` | Base URL of the Hikvision device (e.g., `http://192.168.1.100`) |
| `User` | Username for authentication |
| `Password` | Password for authentication |
| `MaxTimeout` | Maximum request timeout in milliseconds (default: 5000) |

## Dependencies

- .NET 10.0
- RestSharp 113.0+
- Serilog 4.3+
- SeriRestClient 0.1+
- Microsoft.Extensions.DependencyInjection 10.0+
- Microsoft.Extensions.Configuration 10.0+

## Logging

All HTTP requests and responses are automatically logged via Serilog through the integrated SeriRestClient. Configure Serilog in your application to capture detailed API interactions.

## License

See LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Repository

https://bitbucket.org/zyga69/hikisapiclient
