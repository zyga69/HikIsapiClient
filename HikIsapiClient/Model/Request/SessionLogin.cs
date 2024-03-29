using System.Xml.Serialization;

namespace HikIsapi.Client.Model.Request
{
    [XmlRoot(ElementName = "SessionLogin", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
    public class SessionLogin
    {
        [XmlElement(ElementName = "userName")]
        public required string UserName { get; set; }

        [XmlElement(ElementName = "password")]
        public required string Password { get; set; }

        [XmlElement(ElementName = "sessionID")]
        public required string SessionID { get; set; }

        [XmlElement(ElementName = "isSupportSessionTag")]
        public bool IsSupportSessionTag { get; set; }

        [XmlElement(ElementName = "isSessionIDValidLongTerm")]
        public bool IsSessionIDValidLongTerm { get; set; }

        [XmlElement(ElementName = "sessionIDVersion")]
        public int SessionIDVersion { get; set; }
    }

}