using HikIsapi.Client;

namespace HikIsapiTest
{
    public class App
    {
        private readonly IIsapi _isapi;

        public App(IIsapi isapi)
        {
            _isapi = isapi;
        }

        public void Run()
        {
            _isapi.DoorOpen(1);
        }
    }
}