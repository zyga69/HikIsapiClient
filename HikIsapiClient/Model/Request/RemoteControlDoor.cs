using System.Xml.Serialization;

namespace HikIsapi.Client.Model
{

    [XmlRoot(ElementName = "RemoteControlDoor", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
    public class RemoteControlDoor
    {
        public RemoteControlDoor()
        {
            Cmd = "open";
        }
        [XmlElement(ElementName = "cmd")]
        public string Cmd { get; set; }
    }
}