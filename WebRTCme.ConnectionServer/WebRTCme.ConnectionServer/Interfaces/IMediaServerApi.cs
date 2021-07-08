using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Utilme;

namespace WebRTCme.ConnectionServer
{
    public interface IMediaServerApi
    {
        //delegate Task NotifyDelegate(string method, object data);

        Task<Result<Unit>> ConnectAsync(Guid id, string name, string room);
        Task<Result<Unit>> DisconnectAsync(Guid id);

        Task<Result<object>> CallAsync(string method, object data);

        event IMediaServerNotify.NotifyDelegateAsync NotifyEventAsync; 


        //Task<Result<string>> JoinAsync(Guid id, string name, string room);

        //Task<Result<Unit>> LeaveAsync(Guid id);

    }
}
