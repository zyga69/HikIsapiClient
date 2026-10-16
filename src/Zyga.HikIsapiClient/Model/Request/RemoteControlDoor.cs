using System.Xml.Serialization;

namespace Zyga.HikIsapiClient.Model.Request;

/// <summary>
/// Request body for the ISAPI remote-control door-open operation.
/// </summary>
[XmlRoot(ElementName = "RemoteControlDoor", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
public class RemoteControlDoor
{
    /// <summary>
    /// Initializes a new instance with the <c>cmd</c> element set to <c>"open"</c>.
    /// </summary>
    public RemoteControlDoor()
    {
        Cmd = "open";
    }

    /// <summary>
    /// Gets or sets the door command, e.g. <c>"open"</c>.
    /// </summary>
    [XmlElement(ElementName = "cmd")]
    public string Cmd { get; set; }
}
