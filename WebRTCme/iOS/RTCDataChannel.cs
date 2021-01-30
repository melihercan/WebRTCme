using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCDataChannel : ApiBase, IRTCDataChannel, Webrtc.IRTCDataChannelDelegate
    {
        public static IRTCDataChannel Create(Webrtc.RTCDataChannel nativeDataChannel) => 
            new RTCDataChannel(nativeDataChannel);

        private RTCDataChannel(Webrtc.RTCDataChannel nativeDataChannel) : base(nativeDataChannel) { }

        public BinaryType BinaryType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint BufferedAmount => (uint)((Webrtc.RTCDataChannel)NativeObject).BufferedAmount;

        public uint BufferedAmountLowThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ushort Id => (ushort)((Webrtc.RTCDataChannel)NativeObject).ChannelId;

        public string Label => ((Webrtc.RTCDataChannel)NativeObject).Label;

        public ushort? MaxPacketLifeTime => ((Webrtc.RTCDataChannel)NativeObject).MaxPacketLifeTime;

        public ushort? MaxRetransmits => ((Webrtc.RTCDataChannel)NativeObject).MaxRetransmits;

        public bool Negotiated => ((Webrtc.RTCDataChannel)NativeObject).IsNegotiated;

        public bool Ordered => ((Webrtc.RTCDataChannel)NativeObject).IsOrdered;

        public string Protocol => ((Webrtc.RTCDataChannel)NativeObject).Protocol;

        public RTCDataChannelState ReadyState => ((Webrtc.RTCDataChannel)NativeObject).ReadyState.FromNative();

        public event EventHandler OnBufferedAmountLow;
        public event EventHandler OnClose;
        public event EventHandler OnClosing;
        public event EventHandler<IErrorEvent> OnError;
        public event EventHandler<IMessageEvent> OnMessage;
        public event EventHandler OnOpen;

        public void Close() => ((Webrtc.RTCDataChannel)NativeObject).Close();

        public void Send() => throw new NotImplementedException();

        #region NativeEvents
        public void DataChannelDidChangeState(Webrtc.RTCDataChannel dataChannel)
        {
            switch (ReadyState)
            {
                case RTCDataChannelState.Open:
                    OnOpen?.Invoke(this, EventArgs.Empty);
                    break;
                case RTCDataChannelState.Closing:
                    OnClosing?.Invoke(this, EventArgs.Empty);
                    break;
                case RTCDataChannelState.Closed:
                    OnClose?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        public void DidReceiveMessageWithBuffer(Webrtc.RTCDataChannel dataChannel, Webrtc.RTCDataBuffer buffer)
        {

        }

        public void DidChangeBufferedAmount(Webrtc.RTCDataChannel dataChannel, ulong amount)
        {

        }

        #endregion
    }
}
