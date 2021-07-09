using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Connection.MediaSoup
{
    public interface IMediaSoupServerNotify
    {
        delegate Task NotifyDelegateAsync(string method, object data);

        Task OnNotifyAsync(string method, object data);

    }
}
