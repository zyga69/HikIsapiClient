using HikIsapi.Client;

namespace hikisapiclient
{
    /// <summary>
    /// Represents the configuration settings for an ISAPI client.
    /// </summary>
    public class IsapiConfig : IIsapiConfig
    {
        /// <inheritdoc/>
        public required string Name { get; set; }

        /// <inheritdoc/>
        public required string Address { get; set; }

        /// <inheritdoc/>
        public required string User { get; set; }

        /// <inheritdoc/>
        public required string Password { get; set; }

        /// <inheritdoc/>
        public int? MaxTimeout { get; set; }
    }
}