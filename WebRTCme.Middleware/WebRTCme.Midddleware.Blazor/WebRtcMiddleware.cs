using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRtcMiddlewareBlazor;

namespace WebRTCme.Middleware.Blazor
{
    public static class WebRtcMiddleware
    {
        internal static IWebRtc WebRtc { get; private set; }

        public static void Initialize()
        {
            WebRtc = CrossWebRtc.Current;
        }

        public static IRoomService CreateRoomService()
        {
            return new RoomService();
        }

        public static void Cleanup()
        {
            WebRtc.Cleanup();
        }

    }
}
