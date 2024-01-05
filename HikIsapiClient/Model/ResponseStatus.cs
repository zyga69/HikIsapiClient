using System.Xml.Serialization;

namespace HikIsapi.Client.Model
{

    [XmlRoot(ElementName = "ResponseStatus", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
    public class ResponseStatus
    {
        [XmlElement(ElementName = "requestURL")]
        public string RequestUrl { get; set; }

        [XmlElement(ElementName = "statusCode")]
        public string StatusCode { get; set; }

        [XmlElement(ElementName = "statusString")]
        public int StatusString { get; set; }

        [XmlElement(ElementName = "subStatusCode")]
        public bool SubStatusCode { get; set; }
    }
}