using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Shared.SipSorcery.Custom;

namespace WebRTCme.Shared.SipSorcery
{
    internal class RTCDataChannel : NativeBase<SIPSorcery.Net.RTCDataChannel>, IRTCDataChannel
    {
        public RTCDataChannel(SIPSorcery.Net.RTCDataChannel nativeDataChannel) : base(nativeDataChannel)
        {
            NativeObject = nativeDataChannel;

            NativeObject.onopen += NativeOnOpen;
            NativeObject.onclose += NativeOnClose;
            NativeObject.onmessage += NativeOnMessage;
            NativeObject.onerror += NativeOnError;
        }


        public BinaryType BinaryType
        {
            get => (BinaryType)Enum.Parse(typeof(BinaryType), NativeObject.binaryType);
            set => NativeObject.binaryType = value.ToString();
        }

        public uint BufferedAmount => (uint)NativeObject.bufferedAmount;

        public uint BufferedAmountLowThreshold
        {
            get => (uint)NativeObject.bufferedAmountLowThreshold;
            set => NativeObject.bufferedAmountLowThreshold = value;
        }

        public ushort Id => NativeObject.id.Value;

        public string Label => NativeObject.label;

        public ushort? MaxPacketLifeTime => NativeObject.maxPacketLifeTime;

        public ushort? MaxRetransmits => NativeObject.maxRetransmits;

        public bool Negotiated => NativeObject.negotiated;

        public bool Ordered => NativeObject.ordered;

        public string Protocol => NativeObject.protocol;

        public RTCDataChannelState ReadyState => NativeObject.readyState.FromNative();

        public event EventHandler OnBufferedAmountLow;
        public event EventHandler OnClose;
        public event EventHandler OnClosing;
        public event EventHandler<IErrorEvent> OnError;
        public event EventHandler<IMessageEvent> OnMessage;
        public event EventHandler OnOpen;

        public void Close()
        {
            NativeObject.close();
        }

        public void Dispose()
        {
            NativeObject.onopen -= NativeOnOpen;
            NativeObject.onclose -= NativeOnClose;
            NativeObject.onmessage -= NativeOnMessage;
            NativeObject.onerror -= NativeOnError;
        }

        public void Send(object data)
        {
            //// TODO: binary or string data????
            var bytes = data as byte[];
            NativeObject.send(bytes);
        }

        #region NativeEvents
        private void NativeOnOpen()
        {
            OnOpen?.Invoke(this, EventArgs.Empty);
        }

        private void NativeOnClose()
        {
            OnClose?.Invoke(this, EventArgs.Empty);
        }

        private void NativeOnMessage(SIPSorcery.Net.RTCDataChannel dc,
            SIPSorcery.Net.DataChannelPayloadProtocols protocol, byte[] data)
        {
            OnMessage?.Invoke(this, new MessageEvent(protocol switch
            {
                SIPSorcery.Net.DataChannelPayloadProtocols.WebRTC_DCEP => throw new NotImplementedException(),
                SIPSorcery.Net.DataChannelPayloadProtocols.WebRTC_String => Encoding.UTF8.GetString(data),
                SIPSorcery.Net.DataChannelPayloadProtocols.WebRTC_Binary_Partial => data,
                SIPSorcery.Net.DataChannelPayloadProtocols.WebRTC_Binary => data,
                SIPSorcery.Net.DataChannelPayloadProtocols.WebRTC_String_Partial => Encoding.UTF8.GetString(data),
                SIPSorcery.Net.DataChannelPayloadProtocols.WebRTC_String_Empty => throw new NotImplementedException(),
                SIPSorcery.Net.DataChannelPayloadProtocols.WebRTC_Binary_Empty => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            }));
        }

        private void NativeOnError(string obj)
        {
            OnError?.Invoke(this, new ErrorEvent(obj));
        }



        #endregion
    }

}
