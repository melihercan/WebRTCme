using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Services;

namespace WebRTCme.Middleware
{
    public class WebRtcMiddleware : IWebRtcMiddleware
    {
        public IWebRtc WebRtc { get; private set; }

        public WebRtcMiddleware(IWebRtc webRtc)
        {
            WebRtc = webRtc;
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
                    WebRtc.Dispose();
                }

                _isDisposed = true;
            }
        }


        #endregion
    }
}
