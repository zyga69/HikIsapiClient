using System.Xml.Serialization;

namespace Zyga.HikIsapiClient.Model.Response;

/// <summary>
/// Response body returned by the ISAPI session-login operation.
/// </summary>
[XmlRoot(ElementName = "SessionLoginCap", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
public class SessionLogin
{
    /// <summary>
    /// Gets or sets the numeric login status value.
    /// </summary>
    [XmlElement(ElementName = "statusValue")]
    public int StatusValue { get; set; }

    /// <summary>
    /// Gets or sets the login status string.
    /// </summary>
    [XmlElement(ElementName = "statusString")]
    public string? StatusString { get; set; }

    /// <summary>
    /// Gets or sets whether the account is still using its default password.
    /// </summary>
    [XmlElement(ElementName = "isDefaultPassword")]
    public string? IsDefaultPassword { get; set; }

    /// <summary>
    /// Gets or sets whether the device flags the current password as weak.
    /// </summary>
    [XmlElement(ElementName = "isRiskPassword")]
    public string? IsRiskPassword { get; set; }

    /// <summary>
    /// Gets or sets whether the account has been activated.
    /// </summary>
    [XmlElement(ElementName = "isActivated")]
    public string? IsActivated { get; set; }
}
