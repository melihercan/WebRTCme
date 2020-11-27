using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCDataChannel
    {
        BinaryType BinaryType { get; set; }

        uint BufferedAmount { get; }
        
        uint BufferedAmountLowThreshold { get; set; }

        ushort? Id { get; }

        string Label { get; }

        ushort? MaxPacketLifeTime { get; }

        ushort? MaxRetransmits { get; }

        bool Negotiated { get; }

        bool Ordered { get; }

        string Protocol { get; }

        event EventHandler OnBufferedAmountLow;

        event EventHandler OnClose;

        event EventHandler OnClosing;

        event EventHandler<IErrorEvent> OnError;

        event EventHandler<IMessageEvent> OnMessage;

        event EventHandler OnOpen;

    }
}
