using System.Xml.Serialization;

namespace HikIsapi.Client.Model
{

    [XmlRoot(ElementName = "SessionLoginCap", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
    public class SessionLoginCap
    {

        [XmlElement(ElementName = "sessionID")]
        public string SessionID { get; set; }

        [XmlElement(ElementName = "challenge")]
        public string Challenge { get; set; }

        [XmlElement(ElementName = "iterations")]
        public int Iterations { get; set; }

        [XmlElement(ElementName = "isIrreversible")]
        public bool IsIrreversible { get; set; }

        [XmlElement(ElementName = "salt")]
        public string Salt { get; set; }

        [XmlElement(ElementName = "sessionIDVersion")]
        public int SessionIDVersion { get; set; }

    }
}