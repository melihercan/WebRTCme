using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.SignallingServerProxy;

namespace WebRTCme.Middleware
{
    public interface ISignallingServer : /*IAsyncInitialization,*/ IAsyncDisposable
    {
        Task<string[]> GetTurnServerNamesAsync();
    }
}
