using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMessageEvent
    {
        object Data { get; }

        string Origin { get; }

        string LastEventId { get; }

        object Source { get; }

        /*MessagePort*/object[] Ports { get; }
    }
}
