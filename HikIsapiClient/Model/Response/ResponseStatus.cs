using System.Xml.Serialization;

namespace HikIsapi.Client.Model.Response
{

    public enum ResponseStatusString
    {
        [XmlEnum("OK")]
        OK = 1
    }

    public enum ResponseSubStatusString
    {
        [XmlEnum("ok")]
        ok
    }

    [XmlRoot(ElementName = "ResponseStatus", Namespace = "http://www.std-cgi.com/ver10/XMLSchema")]
    public class ResponseStatus
    {
        [XmlElement(ElementName = "requestURL")]
        public  string RequestUrl { get; set; }

        [XmlElement(ElementName = "statusCode")]
        public  int StatusCode { get; set; }

        [XmlElement(ElementName = "statusString")]
        public  ResponseStatusString StatusString { get; set; }

        [XmlElement(ElementName = "subStatusCode")]
        public ResponseSubStatusString? SubStatusCode { get; set; }
    }
}