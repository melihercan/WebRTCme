using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.ConnectionServer
{
    public interface IMediaServerNotify
    {
        delegate Task NotifyDelegateAsync(string method, object data);

        Task OnNotifyAsync(string method, object data);
    }
}
