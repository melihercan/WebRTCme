using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.Services
{
    class ConnectionFactory : IConnectionFactory
    {
        readonly IServiceProvider _serviceProvider;

        public ConnectionFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public IConnection SelectConnection(ConnectionType connectionType) =>
            connectionType switch
            {
                ConnectionType.Signaling => _serviceProvider.GetService(typeof(SignalingConnection)) as IConnection,
                ConnectionType.MediaSoup => _serviceProvider.GetService(typeof(MediaSoupConnection)) as IConnection,
                _ => throw new NotSupportedException($"'{connectionType}' is not supported")
            };
    }
}
