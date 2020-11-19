using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCDataChannel
    {
        Task<BinaryType> BinaryType { get; set; }

        Task<uint> BufferedAmount { get; }
        
        Task<uint> BufferedAmountLowThreshold { get; set; }

        Task<ushort?> Id { get; }

        Task<string> Label { get; }

        Task<ushort?> MaxPacketLifeTime { get; }

        Task<ushort?> MaxRetransmits { get; }

        Task<bool> Negotiated { get; }

        Task<bool> Ordered { get; }

        Task<string> Protocol { get; }

        event EventHandler OnBufferedAmountLow;

        event EventHandler OnClose;

        event EventHandler OnClosing;

        event EventHandler<IErrorEvent> OnError;

        event EventHandler<IMessageEvent> OnMessage;

        event EventHandler OnOpen;

    }
}
