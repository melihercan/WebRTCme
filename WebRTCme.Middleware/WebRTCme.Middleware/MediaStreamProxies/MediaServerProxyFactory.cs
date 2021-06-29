using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware.MediaStreamProxies
{
    class MediaServerProxyFactory
    {
        readonly IServiceProvider _serviceProvider;

        public MediaServerProxyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMediaServerProxy Create(MediaServer mediaServer) =>
            mediaServer switch
            {
                MediaServer.MediaSoup => _serviceProvider.GetService(typeof(MediaSoupProxy)) as IMediaServerProxy,
                _ => throw new NotSupportedException($"'{mediaServer}' is not supported")
            };
    }
}
