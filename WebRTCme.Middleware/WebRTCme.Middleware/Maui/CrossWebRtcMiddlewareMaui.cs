namespace WebRTCme.Middleware
{
    public static class CrossWebRtcMiddlewareMaui
    {
        public static IWebRtcMiddleware Current => _webRtcMiddleware.Value;
       
        private static readonly Lazy<IWebRtcMiddleware> _webRtcMiddleware = new Lazy<IWebRtcMiddleware>(() => 
            CreateWebRtcMiddleware());

        private static IWebRtcMiddleware CreateWebRtcMiddleware()
        {
            var webRtc = CrossWebRtc.Current;
            return new WebRtcMiddleware(webRtc);
        }

    }
}
