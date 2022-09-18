using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Desktop.Windows.Custom;

namespace WebRTCme.Bindings.Desktop.Windows.Api
{
    internal class RTCDataChannel : NativeBase<SIPSorcery.Net.RTCDataChannel>, IRTCDataChannel
    {
        public RTCDataChannel(SIPSorcery.Net.RTCDataChannel nativeDataChannel) : base(nativeDataChannel) =>
            NativeObject = nativeDataChannel;

        public BinaryType BinaryType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint BufferedAmount => throw new NotImplementedException();

        public uint BufferedAmountLowThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ushort Id => throw new NotImplementedException();

        public string Label => throw new NotImplementedException();

        public ushort? MaxPacketLifeTime => throw new NotImplementedException();

        public ushort? MaxRetransmits => throw new NotImplementedException();

        public bool Negotiated => throw new NotImplementedException();

        public bool Ordered => throw new NotImplementedException();

        public string Protocol => throw new NotImplementedException();

        public RTCDataChannelState ReadyState => throw new NotImplementedException();

        public event EventHandler OnBufferedAmountLow;
        public event EventHandler OnClose;
        public event EventHandler OnClosing;
        public event EventHandler<IErrorEvent> OnError;
        public event EventHandler<IMessageEvent> OnMessage;
        public event EventHandler OnOpen;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Send(object data)
        {
            throw new NotImplementedException();
        }
    }
}
