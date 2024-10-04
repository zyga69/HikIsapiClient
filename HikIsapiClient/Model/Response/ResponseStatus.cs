using System.Xml.Serialization;

namespace HikIsapi.Client.Model.Response
{

    public enum ResponseStatusString
    {
        [XmlEnum("OK")]
        OK = 1,
        [XmlEnum("Invalid Operation")]
        InvalidOperation = 2
    }

    public enum ResponseSubStatusString
    {
        [XmlEnum("ok")]
        ok,

        [XmlEnum("invalidOperation")]
        invalidOperation,

        [XmlEnum("badAuthorization")]
        badAuthorization
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