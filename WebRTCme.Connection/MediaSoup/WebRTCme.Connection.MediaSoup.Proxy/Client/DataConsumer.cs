using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class DataConsumer
    {
        readonly string _id;
        readonly string _dataProducerId;
        readonly IRTCDataChannel _dataChannel;
        readonly SctpStreamParameters _sctpStreamParameters;
        readonly object _appData;
        bool _closed;

        public DataConsumer(string id, string dataProducerId, IRTCDataChannel dataChannel, 
            SctpStreamParameters sctpStreamParameters, object appData)
        {
            _id = id;
            _dataProducerId = dataProducerId;
            _dataChannel = dataChannel;
            _sctpStreamParameters = sctpStreamParameters;
            _appData = appData;

            HandleDataChannel();
        }

        public event EventHandler OnOpen;
        public event EventHandler OnClose;
        public event EventHandler<string> OnError;
        public event EventHandler OnTransportClosed;
        public event EventHandler<object> OnMessage;

        public string Id => _id;
        public string DataProducerId => _dataProducerId;
        public bool Closed => _closed;
        public SctpStreamParameters SctpStreamParameters => _sctpStreamParameters;
        public RTCDataChannelState DataChannelState => _dataChannel.ReadyState;
        public string Label => _dataChannel.Label;
        public string Protocol => _dataChannel.Protocol;
        public BinaryType BinaryType
        {
            get => _dataChannel.BinaryType;
            set => _dataChannel.BinaryType = value;
        }
        public object AppData => _appData;

        public void Close()
        {
            if (_closed)
                return;

            Console.WriteLine("close()");

            _closed = true;

            DestroyDataChannel();
            _dataChannel.Close();

            OnClose?.Invoke(this, EventArgs.Empty);
        }

        public void TransportClosed()
        {
            if (_closed)
                return;

            Console.WriteLine("TransportClosed()");

            _closed = true;

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

        void DataChannel_OnMessage(object sender, IMessageEvent e)
        {
            throw new NotImplementedException();
        }

        void DataChannel_OnClose(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void DataChannel_OnError(object sender, IErrorEvent e)
        {
            throw new NotImplementedException();
        }

        void DataChannel_OnOpen(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
