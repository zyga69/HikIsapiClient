namespace Zyga.HikIsapiClient;

/// <summary>
/// Entry point for interacting with a Hikvision device over ISAPI.
/// </summary>
public interface IIsapi
{
    /// <summary>
    /// Logs in to the device unconditionally, regardless of current session state.
    /// </summary>
    /// <returns>An authenticated client for the current session.</returns>
    IIsapiAuthenticated Login();

    /// <summary>
    /// Logs in only if there is no active session; otherwise reuses the existing one.
    /// </summary>
    /// <returns>An authenticated client for the current session.</returns>
    IIsapiAuthenticated EnsureLoggedIn();

    /// <summary>
    /// Checks whether the device is reachable by requesting its login capabilities.
    /// </summary>
    /// <returns><c>true</c> if the device responded successfully; otherwise <c>false</c>.</returns>
    bool IsAvailable();
}

/// <summary>
/// An ISAPI client with an active, authenticated session.
/// </summary>
public interface IIsapiAuthenticated
{
    /// <summary>
    /// Triggers the remote control door-open operation for the given door.
    /// </summary>
    /// <param name="doorNo">The door number to open.</param>
    /// <returns>The same authenticated client, for chaining.</returns>
    IIsapiAuthenticated DoorOpen(int doorNo);

    /// <summary>
    /// Sends a heartbeat to keep the current session alive.
    /// </summary>
    /// <returns>The same authenticated client, for chaining.</returns>
    IIsapiAuthenticated SessionHeartBeat();

    /// <summary>
    /// Gets the number of heartbeats sent during the current session.
    /// </summary>
    int HeartBeatCount { get; }

    /// <summary>
    /// Ends the current session.
    /// </summary>
    /// <returns>The unauthenticated client.</returns>
    IIsapi LogOut();
}
