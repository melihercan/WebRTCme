using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IWebRtcMiddleware : IDisposable
    {
        Task<ISignallingServerService> CreateSignallingServerServiceAsync(/*string signallingServerBaseUrl*/
            IConfiguration configuration,
            ILogger<ISignallingServerService> logger,
            IJSRuntime jsRuntime = null);

        Task<IMediaStreamService> CreateMediaStreamServiceAsync(IJSRuntime jsRuntime = null);

    }
}
