using Microsoft.Extensions.Configuration;

namespace WebRTCme.Connection.Signaling.Proxy
{
    /// <summary>
    /// The default: <c>SignalingServer:BaseUrl</c> from configuration, which is where it has always
    /// come from.
    /// </summary>
    /// <remarks>
    /// Read on every access rather than captured, so a configuration source that can be rebuilt at
    /// runtime keeps working the way its owner expects. An app that needs something configuration
    /// cannot express registers its own <see cref="ISignalingServerUrlProvider"/> instead.
    /// </remarks>
    class ConfigurationSignalingServerUrlProvider : ISignalingServerUrlProvider
    {
        readonly IConfiguration _configuration;

        public ConfigurationSignalingServerUrlProvider(IConfiguration configuration) =>
            _configuration = configuration;

        public string BaseUrl => _configuration["SignalingServer:BaseUrl"];
    }
}
