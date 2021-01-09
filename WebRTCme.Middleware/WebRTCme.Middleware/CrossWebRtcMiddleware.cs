using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public static class CrossWebRtcMiddleware
    {
        public static IWebRtcMiddleware Current => _webRtcMiddleware.Value;

        private static readonly Lazy<IWebRtcMiddleware> _webRtcMiddleware = new Lazy<IWebRtcMiddleware>(() => 
            CreateWebRtcMiddleware());

        private static IWebRtcMiddleware CreateWebRtcMiddleware() => new WebRtcMiddleware();
    }
}
