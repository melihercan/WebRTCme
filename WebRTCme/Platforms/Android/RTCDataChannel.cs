using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;
using System.Text;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCDataChannel : NativeBase<Webrtc.DataChannel>, IRTCDataChannel, Webrtc.DataChannel.IObserver
    {
        public RTCDataChannel(Webrtc.DataChannel nativeDataChannel) : base(nativeDataChannel)
        {
            nativeDataChannel.RegisterObserver(this);
        }

        public BinaryType BinaryType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint BufferedAmount => (uint)NativeObject.BufferedAmount();

        public uint BufferedAmountLowThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ushort Id => (ushort)NativeObject.Id();

        public string Label => NativeObject.Label();

        public ushort? MaxPacketLifeTime => throw new NotImplementedException();

        public ushort? MaxRetransmits => throw new NotImplementedException();

        public bool Negotiated => throw new NotImplementedException();

        public bool Ordered => throw new NotImplementedException();

        public string Protocol => throw new NotImplementedException();

        public RTCDataChannelState ReadyState => NativeObject.InvokeState().FromNative();

        public event EventHandler OnBufferedAmountLow;
        public event EventHandler OnClose;
        public event EventHandler OnClosing;
        public event EventHandler<IErrorEvent> OnError;
        public event EventHandler<IMessageEvent> OnMessage;
        public event EventHandler OnOpen;

        public void Close() => NativeObject.Close();

        public void Send(object data)
        {
            DataChannel.Buffer buffer = null;

            if (data.GetType() == typeof(byte[]))
                buffer = new DataChannel.Buffer(Java.Nio.ByteBuffer.Wrap((byte[])data), true);
            else if (data.GetType() == typeof(string))
                buffer = new DataChannel.Buffer(Java.Nio.ByteBuffer.Wrap(
                    Encoding.UTF8.GetBytes((string)data)), false);
            else
                throw new ArgumentException($"{data.GetType()} type is not supported");

            var state = NativeObject.InvokeState();
            var result = NativeObject.Send(buffer);
        }

        #region NativeEvents
        public void OnBufferedAmountChange(long p0)
        {
            //if (p0 < ???)
                //OnBufferedAmountLow?.Invoke(this, EventArgs.Empty);
        }

        void DataChannel.IObserver.OnMessage(DataChannel.Buffer p0)
        {
            var bytes = new byte[p0.Data.Remaining()];
            p0.Data.Get(bytes);
            if (p0.Binary)
                OnMessage?.Invoke(this, new MessageEvent(bytes));
            else
                OnMessage?.Invoke(this, new MessageEvent(Encoding.UTF8.GetString(bytes)));
        }

        public void OnStateChange()
        {
            var nativeState = NativeObject.InvokeState();
            if (nativeState == Webrtc.DataChannel.State.Open)
                OnOpen?.Invoke(this, EventArgs.Empty);
            else if (nativeState == Webrtc.DataChannel.State.Closing)
                OnClosing?.Invoke(this, EventArgs.Empty);
            else if (nativeState == Webrtc.DataChannel.State.Closed)
                OnClose?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}