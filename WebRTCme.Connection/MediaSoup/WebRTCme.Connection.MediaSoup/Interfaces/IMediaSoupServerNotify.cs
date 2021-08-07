using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Connection.MediaSoup
{
    public interface IMediaSoupServerNotify
    {
        delegate void Accept(object data = null);
        delegate void Reject(int error, string errorReason);

        delegate Task NotifyDelegateAsync(string method, object data);
        delegate Task RequestDelegateAsync(string method, object data, Accept accept, Reject reject);


        Task OnNotifyAsync(string method, object data);
        Task OnRequestAsync(string method, object data, Accept accept, Reject reject);

    }
}
