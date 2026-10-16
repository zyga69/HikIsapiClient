# Zyga.HikIsapiClient

[![CI](https://github.com/zyga69/HikIsapiClient/actions/workflows/ci.yml/badge.svg)](https://github.com/zyga69/HikIsapiClient/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Zyga.HikIsapiClient.svg)](https://www.nuget.org/packages/Zyga.HikIsapiClient)
[![Downloads](https://img.shields.io/nuget/dt/Zyga.HikIsapiClient.svg)](https://www.nuget.org/packages/Zyga.HikIsapiClient)
[![codecov](https://codecov.io/gh/zyga69/HikIsapiClient/branch/main/graph/badge.svg)](https://codecov.io/gh/zyga69/HikIsapiClient)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A .NET client library for interacting with Hikvision devices using the ISAPI protocol, with integrated Serilog logging via [Zyga.SeriRestClient](https://www.nuget.org/packages/Zyga.SeriRestClient).

## Features

- 🔐 **Session Management** - Automatic login/logout handling with session keep-alive
- 🚪 **Door Control** - Remote door control operations
- 📝 **Logging** - Request/response logging via Zyga.SeriRestClient's Serilog integration
- 🔄 **State Management** - Smart session state handling with `EnsureLoggedIn()`
- 🔌 **Dependency Injection** - First-class support for Microsoft.Extensions.DependencyInjection
- 🎯 **Keyed Services** - Support for multiple Hikvision device instances
- ⚙️ **Configuration** - Easy configuration through IConfiguration

## Install

```bash
dotnet add package Zyga.HikIsapiClient
```

Targets .NET 8, .NET 9 and .NET 10.

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
using Zyga.HikIsapiClient.ExtensionMethods;

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

## Sample

A runnable console sample lives in [`samples/HikIsapiClient.Sample`](samples/HikIsapiClient.Sample) —
point its `appsettings.json` at your own device before running it.

## Requirements

- .NET 8, .NET 9, or .NET 10
- RestSharp 114.0.0+
- Serilog 4.4.0+
- Zyga.SeriRestClient 0.1.1+

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE)

## Version

This package uses [MinVer](https://github.com/adamralph/minver) for semantic versioning,
derived from `v`-prefixed git tags (e.g. `v1.2.3`). There is no `<Version>` element
anywhere in the source — maintainers cut a release by pushing a tag.
