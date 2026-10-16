using System.Xml.Serialization;

namespace Zyga.HikIsapiClient.Model.Request;

/// <summary>
/// Request body for the ISAPI session-login operation.
/// </summary>
[XmlRoot(ElementName = "SessionLogin", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
public class SessionLogin
{
    /// <summary>
    /// Gets or sets the account user name.
    /// </summary>
    [XmlElement(ElementName = "userName")]
    public required string UserName { get; set; }

    /// <summary>
    /// Gets or sets the challenge-response password hash.
    /// </summary>
    [XmlElement(ElementName = "password")]
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the session ID returned by the login-capabilities response.
    /// </summary>
    [XmlElement(ElementName = "sessionID")]
    public required string SessionID { get; set; }

    /// <summary>
    /// Gets or sets whether the device should support session tagging.
    /// </summary>
    [XmlElement(ElementName = "isSupportSessionTag")]
    public bool IsSupportSessionTag { get; set; }

    /// <summary>
    /// Gets or sets whether the session ID should remain valid long-term.
    /// </summary>
    [XmlElement(ElementName = "isSessionIDValidLongTerm")]
    public bool IsSessionIDValidLongTerm { get; set; }

    /// <summary>
    /// Gets or sets the session ID protocol version.
    /// </summary>
    [XmlElement(ElementName = "sessionIDVersion")]
    public int SessionIDVersion { get; set; }
}
