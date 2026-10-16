using System.Xml.Serialization;

namespace Zyga.HikIsapiClient.Model.Response;

/// <summary>
/// Response body returned by the ISAPI login-capabilities operation, describing the
/// challenge a client must answer to log in.
/// </summary>
[XmlRoot(ElementName = "SessionLoginCap", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
public class SessionLoginCap
{
    /// <summary>
    /// Gets or sets the session ID to use when submitting the login request.
    /// </summary>
    [XmlElement(ElementName = "sessionID")]
    public required string SessionID { get; set; }

    /// <summary>
    /// Gets or sets the challenge string the client must hash the password against.
    /// </summary>
    [XmlElement(ElementName = "challenge")]
    public required string Challenge { get; set; }

    /// <summary>
    /// Gets or sets the number of hash iterations the device requires.
    /// </summary>
    [XmlElement(ElementName = "iterations")]
    public int Iterations { get; set; }

    /// <summary>
    /// Gets or sets whether the device uses the irreversible hashing scheme.
    /// </summary>
    [XmlElement(ElementName = "isIrreversible")]
    public bool IsIrreversible { get; set; }

    /// <summary>
    /// Gets or sets the salt to use when hashing the password.
    /// </summary>
    [XmlElement(ElementName = "salt")]
    public required string Salt { get; set; }

    /// <summary>
    /// Gets or sets the session ID protocol version.
    /// </summary>
    [XmlElement(ElementName = "sessionIDVersion")]
    public int SessionIDVersion { get; set; }
}
