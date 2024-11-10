using HikIsapi.Client;

namespace hikisapiclient
{
    public class IsapiConfig : IIsapiConfig
    {
        public required string Name { get; set; }
        public required string Address { get; set; }

        public required string User { get; set; }

        public required string Password { get; set; }
        public int? MaxTimeout { get; set; }
    }
}