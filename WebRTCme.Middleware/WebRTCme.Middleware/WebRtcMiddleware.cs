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

        public WebRtcMiddleware(IWebRtc webRtc)
        {
            WebRtc = webRtc;
        }

        public Task<ISignallingServerService> CreateSignallingServerServiceAsync(string signallingServerBaseUrl, 
            IJSRuntime jsRuntime)
        {
            return SignallingServerService.CreateAsync(signallingServerBaseUrl, jsRuntime);
        }

        public Task<IMediaStreamService> CreateMediaStreamServiceAsync(IJSRuntime jsRuntime)
        {
            return MediaStreamService.CreateAsync(jsRuntime);
        }

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
