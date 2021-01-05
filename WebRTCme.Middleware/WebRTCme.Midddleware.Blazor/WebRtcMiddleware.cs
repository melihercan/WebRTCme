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
        internal static string SignallingServerBaseUrl { get; private set; }

        public static void Initialize(string signallingServerBaseUrl)
        {
            WebRtc = CrossWebRtc.Current;
            SignallingServerBaseUrl = signallingServerBaseUrl;
        }

        public static Task<IRoomService> CreateRoomServiceAsync()
        {
            return RoomService.CreateAsync();
        }

        public static void Cleanup()
        {
            WebRtc.Cleanup();
        }

    }
}
