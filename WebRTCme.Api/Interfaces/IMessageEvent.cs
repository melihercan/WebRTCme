using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMessageEvent
    {
        Task<object> Data { get; }

        Task<string> Origin { get; }

        Task<string> LastEventId { get; }

        Task<object> Source { get; }

        /*MessagePort*/Task<object[]> Ports { get; }

    }
}
