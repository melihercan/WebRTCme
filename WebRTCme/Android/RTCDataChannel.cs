using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;

namespace WebRtc.Android
{
    internal class RTCDataChannel : ApiBase, IRTCDataChannel, Webrtc.DataChannel.IObserver
    {

        public static IRTCDataChannel Create(Webrtc.DataChannel nativeDataChannel) =>
            new RTCDataChannel(nativeDataChannel);

        private RTCDataChannel(Webrtc.DataChannel nativeDataChannel) : base(nativeDataChannel)
        {
        }

        public BinaryType BinaryType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint BufferedAmount => (uint)((Webrtc.DataChannel)NativeObject).BufferedAmount();

        public uint BufferedAmountLowThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ushort Id => (ushort)((Webrtc.DataChannel)NativeObject).Id();

        public string Label => ((Webrtc.DataChannel)NativeObject).Label();

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

        public void Close() => ((Webrtc.DataChannel)NativeObject).Close();

        public void Send()
        {
            throw new NotImplementedException();
        }

        #region NativeEvents
        public void OnBufferedAmountChange(long p0)
        {
            throw new NotImplementedException();
        }

        void DataChannel.IObserver.OnMessage(DataChannel.Buffer p0)
        {
            throw new NotImplementedException();
        }

        public void OnStateChange()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}