using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCDataChannel : NativeBase<Webrtc.RTCDataChannel>, IRTCDataChannel, Webrtc.IRTCDataChannelDelegate
    {
        public RTCDataChannel(Webrtc.RTCDataChannel nativeDataChannel) : base(nativeDataChannel) 
        {
            nativeDataChannel.Delegate = this;
        }

        public BinaryType BinaryType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint BufferedAmount => (uint)NativeObject.BufferedAmount;

        public uint BufferedAmountLowThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ushort Id => (ushort)NativeObject.ChannelId;

        public string Label => NativeObject.Label;

        public ushort? MaxPacketLifeTime => NativeObject.MaxPacketLifeTime;

        public ushort? MaxRetransmits => NativeObject.MaxRetransmits;

        public bool Negotiated => NativeObject.IsNegotiated;

        public bool Ordered => NativeObject.IsOrdered;

        public string Protocol => NativeObject.Protocol;

        public RTCDataChannelState ReadyState => NativeObject.ReadyState.FromNative();

        public event EventHandler OnBufferedAmountLow;
        public event EventHandler OnClose;
        public event EventHandler OnClosing;
        public event EventHandler<IErrorEvent> OnError;
        public event EventHandler<IMessageEvent> OnMessage;
        public event EventHandler OnOpen;

        public void Close() => NativeObject.Close();

        public void Send(object data)
        {
            Webrtc.RTCDataBuffer buffer = null;

            if (data.GetType() == typeof(byte[]))
                buffer = new Webrtc.RTCDataBuffer(NSData.FromArray((byte[])data), true);
            else if (data.GetType() == typeof(string))
                buffer = new Webrtc.RTCDataBuffer(NSData.FromString((string)data, NSStringEncoding.UTF8), false);
            else
                throw new ArgumentException($"{data.GetType()} type is not supported");

      var state = NativeObject.ReadyState;
            var result = NativeObject.SendData(buffer);
        }

        #region NativeEvents
        public void DataChannelDidChangeState(Webrtc.RTCDataChannel dataChannel)
        {
            System.Diagnostics.Debug.WriteLine($"%%%%%%%%%%%%%%%%%%%%%%%%%%%%% RTCDataChannelState: {ReadyState}");

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
            if (buffer.IsBinary)
            {
                OnMessage?.Invoke(this, new MessageEvent(buffer.Data.ToArray()));
            }
            else
            {
                OnMessage?.Invoke(this, new MessageEvent(buffer.Data.ToString()));
            }
        }

        public void DidChangeBufferedAmount(Webrtc.RTCDataChannel dataChannel, ulong amount)
        {
            //if (amount < ???)
            //OnBufferedAmountLow?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
