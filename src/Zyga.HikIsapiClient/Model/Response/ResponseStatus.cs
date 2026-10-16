using System.Xml.Serialization;

namespace Zyga.HikIsapiClient.Model.Response;

/// <summary>
/// Top-level status string returned by ISAPI operations.
/// </summary>
public enum ResponseStatusString
{
    /// <summary>The operation succeeded.</summary>
    [XmlEnum("OK")]
    OK = 1,

    /// <summary>The operation failed.</summary>
    [XmlEnum("Invalid Operation")]
    InvalidOperation = 2
}

/// <summary>
/// Sub-status string providing additional detail on an ISAPI operation's outcome.
/// </summary>
public enum ResponseSubStatusString
{
    /// <summary>The operation succeeded.</summary>
    [XmlEnum("ok")]
    ok,

    /// <summary>The requested operation was invalid.</summary>
    [XmlEnum("invalidOperation")]
    invalidOperation,

    /// <summary>The request was not authorized.</summary>
    [XmlEnum("badAuthorization")]
    badAuthorization
}

/// <summary>
/// Generic response body returned by most ISAPI operations (heartbeat, logout, door control).
/// </summary>
[XmlRoot(ElementName = "ResponseStatus", Namespace = "http://www.std-cgi.com/ver10/XMLSchema")]
public class ResponseStatus
{
    /// <summary>
    /// Gets or sets the URL that was requested.
    /// </summary>
    [XmlElement(ElementName = "requestURL")]
    public string? RequestUrl { get; set; }

    /// <summary>
    /// Gets or sets the numeric status code.
    /// </summary>
    [XmlElement(ElementName = "statusCode")]
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the status string.
    /// </summary>
    [XmlElement(ElementName = "statusString")]
    public ResponseStatusString StatusString { get; set; }

    /// <summary>
    /// Gets or sets the sub-status code, if the device returned one.
    /// </summary>
    [XmlElement(ElementName = "subStatusCode")]
    public ResponseSubStatusString? SubStatusCode { get; set; }
}
