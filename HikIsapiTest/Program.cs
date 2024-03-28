

using System.Xml;
using System.Xml.Serialization;
using HikIsapi.Client;
using HikIsapi.Client.Model;

namespace HikIsapi.Test;

internal class Program
{
    private static void Main(string[] args)
    {
        XmlSerializer xsSubmit = new XmlSerializer(typeof(RemoteControlDoor));
        var data = new RemoteControlDoor();
        var xml = "";

        // using (var sww = new StringWriter())
        // {
        //     using (XmlWriter writer = XmlWriter.Create(sww))
        //     {
        //         xsSubmit.Serialize(writer, data);
        //         xml = sww.ToString(); // Your XML
        //     }
        // }

        using (var isapi = new Isapi(new Config()))
        {
            isapi.DoorOpen(1);
        }


    }
}

public class Config : IIsapiConfig
{
    public string Address => "http://192.168.0.163";

    public string User => "admin";

    public string Password => "REDACTED";
}
