using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class DataConsumer
    {
        readonly IRTCDataChannel _dataChannel;

        public DataConsumer(string id, string dataProducerId, IRTCDataChannel dataChannel, 
            SctpStreamParameters sctpStreamParameters, Dictionary<string, object> appData)
        {
            Id = id;
            DataProducerId = dataProducerId;
            _dataChannel = dataChannel;
            SctpStreamParameters = sctpStreamParameters;
            AppData = appData;
            Closed = false;

            HandleDataChannel();
        }

        public event EventHandler OnOpen;
        public event EventHandler OnClose;
        public event EventHandler<IErrorEvent> OnError;
        public event EventHandler OnTransportClosed;
        public event EventHandler<IMessageEvent> OnMessage;

        public string Id { get; }
        public string DataProducerId { get; }
        public bool Closed { get; private set; }
        public SctpStreamParameters SctpStreamParameters { get; }
        public RTCDataChannelState DataChannelState => _dataChannel.ReadyState;
        public string Label => _dataChannel.Label;
        public string Protocol => _dataChannel.Protocol;
        public BinaryType BinaryType
        {
            get => _dataChannel.BinaryType;
            set => _dataChannel.BinaryType = value;
        }
        public Dictionary<string, object> AppData { get; }

        public IRTCDataChannel DataChannel => _dataChannel;

        public void Close()
        {
            if (Closed)
                return;

            Console.WriteLine("close()");

            Closed = true;

            DestroyDataChannel();
            _dataChannel.Close();

            OnClose?.Invoke(this, EventArgs.Empty);
        }

        public void TransportClosed()
        {
            if (Closed)
                return;

            Console.WriteLine("TransportClosed()");

            Closed = true;

            DestroyDataChannel();
            _dataChannel.Close();

            OnTransportClosed?.Invoke(this, EventArgs.Empty);
        }

        void HandleDataChannel()
        {
            _dataChannel.OnOpen += DataChannel_OnOpen;
            _dataChannel.OnError += DataChannel_OnError;
            _dataChannel.OnClose += DataChannel_OnClose;
            _dataChannel.OnMessage += DataChannel_OnMessage;
        }

        void DestroyDataChannel()
        {
            _dataChannel.OnOpen -= DataChannel_OnOpen;
            _dataChannel.OnError -= DataChannel_OnError;
            _dataChannel.OnClose -= DataChannel_OnClose;
            _dataChannel.OnMessage -= DataChannel_OnMessage;
        }

        void DataChannel_OnOpen(object sender, EventArgs e)
        {
            if (Closed)
                return;

            OnOpen?.Invoke(this, e);
        }

        void DataChannel_OnError(object sender, IErrorEvent e)
        {
            if (Closed)
                return;

            OnError?.Invoke(this, e);
        }

        void DataChannel_OnClose(object sender, EventArgs e)
        {
            if (Closed)
                return;

            Closed = true;
            OnClose?.Invoke(this, e);
        }

        void DataChannel_OnMessage(object sender, IMessageEvent e)
        {
            if (Closed)
                return;

            OnMessage?.Invoke(this, e);
        }



    }
}
