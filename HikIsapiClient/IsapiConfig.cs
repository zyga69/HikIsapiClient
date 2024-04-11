using HikIsapi.Client;

namespace hikisapiclient
{
    public class IsapiConfig : IIsapiConfig
    {
        public string Address { get; set; }

        public string User { get; set; }

        public string Password { get; set; }
        public int MaxTimeout { get; set; }
    }
}