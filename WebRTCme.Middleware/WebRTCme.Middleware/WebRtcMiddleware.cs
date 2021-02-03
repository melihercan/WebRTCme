using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRtcMeMiddleware;

namespace WebRTCme.Middleware
{
    public class WebRtcMiddleware : IWebRtcMiddleware
    {
        public static IWebRtc WebRtc { get; private set; }

////        internal static string SignallingServerBaseUrl { get; private set; }


        public WebRtcMiddleware()
        {
            WebRtc = CrossWebRtc.Current;
////            SignallingServerBaseUrl = signallingServerBaseUrl;
        }

        //public static void Initialize(string signallingServerBaseUrl)
        //{
        //}

        public Task<IRoomService> CreateRoomServiceAsync(string signallingServerBaseUrl, IJSRuntime jsRuntime)
        {
            return RoomService.CreateAsync(signallingServerBaseUrl, jsRuntime);
        }

        public Task<IMediaStreamService> CreateMediaStreamServiceAsync(IJSRuntime jsRuntime)
        {
            var service = MediaStreamService.Create(jsRuntime);
            return Task.FromResult(service);
        }

        public Task<ILocalMediaStreamService> CreateLocalMediaStreamServiceAsync(IJSRuntime jsRuntime)
        {
            var service = LocalMediaStreamService.Create(jsRuntime);
            return Task.FromResult(service);
        }


        //public static void Cleanup()
        //{
        //  WebRtc.Cleanup();
        //}


        #region IDisposable
        private bool _isDisposed = false;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed.
                    WebRtc.Cleanup();
                }

                _isDisposed = true;
            }
        }


        #endregion
    }
}
