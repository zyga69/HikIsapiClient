using System.Xml.Serialization;

namespace hikisapiclient.Model.Response
{
    [XmlRoot(ElementName = "SessionLoginCap", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
    public class SessionLogin
    {
        [XmlElement(ElementName = "statusValue")]
        public int StatusValue { get; set; }

        [XmlElement(ElementName = "statusString")]
        public string? StatusString { get; set; }

        [XmlElement(ElementName = "isDefaultPassword")]
        public string? IsDefaultPassword { get; set; }

        [XmlElement(ElementName = "isRiskPassword")]
        public string? IsRiskPassword { get; set; }

        [XmlElement(ElementName = "isActivated")]
        public string? IsActivated { get; set; }
    }
}