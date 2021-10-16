using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Utilme;

namespace WebRTCme.Connection.MediaSoup
{
    public interface IMediaSoupServerApi : IAsyncDisposable
    {
        Task<Result<Unit>> ConnectAsync(Guid id, string name, string room);
        Task<Result<Unit>> DisconnectAsync(Guid id);

        Task<Result<object>> ApiAsync(string method, object data = null);

        event IMediaSoupServerNotify.NotifyDelegateAsync NotifyEventAsync;
        event IMediaSoupServerNotify.RequestDelegateAsync RequestEventAsync;

    }
}
