using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Transport
    {
        TransportOptions _options;
        ExtendedRtpCapabilities _extendedRtpCapabilities;
        CanProduceByKind _canProduceByKind;
        int? _maxSctpMessageSize;



        public string Id { get; }
        public bool Closed { get; private set; }

        public InternalDirection Direction { get; }

        public Handler Handler { get; }

        public ConnectionState ConnectionState { get; private set; }
        public object AppData { get; }

        public event EventHandler<ConnectionState> OnConnectionStateChange;
        public event EventHandlerAsync<DtlsParameters> OnConnectAsync;


        public Transport(InternalDirection direction, TransportOptions options, Handler handler,
            ExtendedRtpCapabilities extendedRtpCapabilities, CanProduceByKind canProduceByKind)
        {
            _options = options;

            Id = options.Id;
            Closed = false;
            Direction = direction;
            Handler = handler;
            ConnectionState = ConnectionState.New;
            AppData = options.AppData;
            _extendedRtpCapabilities = extendedRtpCapabilities;
            _canProduceByKind = canProduceByKind;
            _maxSctpMessageSize = options.SctpParameters is not null ? options.SctpParameters.MaxMessageSize : null;

            //// TODO: CHECK options.AdditionalSettings

            Handler.Run(new HandlerRunOptions
            {
                Direction = Direction,
                IceParameters = options.IceParameters,
                IceCandidates = options.IceCandidates,
                DtlsParameters = options.DtlsParameters,
                SctpParameters = options.SctpParameters,
                IceServers = options.IceServers,
                IceTransportPolicy = options.IceTransportPolicy,
                AdditionalSettings = options.AdditionalSettings,
                ProprietaryConstraints = options.ProprietaryConstraints,
                ExtendedRtpCapabilities = extendedRtpCapabilities
            });

            HandleHandler();
        }

        public async Task<Consumer> Consume(ConsumerOptions options)
        {
            Console.WriteLine("Consume()");

            var rtpParameters = Utils.Clone<RtpParameters>(options.RtpParameters, null);

            if (Closed)
                throw new Exception("closed");
            else if (Direction != InternalDirection.Recv)
                throw new Exception("not a receiving Transport");
            else if (options.Kind != MediaKind.Audio && options.Kind != MediaKind.Video)
                throw new Exception($"invalid kind {options.Kind}");
            else if (ConnectionState == ConnectionState.New)
                throw new Exception($"no connect listener set into this transport");


            var consumer = new Consumer(
                "",
                "",
                "",
                null,
                null,
                null,
                null);


            HandleConsumer(consumer);

            return consumer;
        }


        public async Task<Producer> Produce(ProducerOptions options)
        {

            ///////// TESTING
            var producer = new Producer(
                "",
                "",
                null,
                null,
                null,
                false,
                false,
                false,
                null);


            HandleProducer(producer);

            return producer;

        }


        void HandleHandler()
        {
            Handler.OnConnectionStateChange += Handler_OnConnectionStateChange;
            Handler.OnConnectAsync += Handler_OnConnectAsync;
        }

        async Task Handler_OnConnectAsync(object sender, DtlsParameters e)
        {
            await OnConnectAsync.Invoke(this, e);
        }

        void Handler_OnConnectionStateChange(object sender, ConnectionState connectionState)
        {
            if (ConnectionState == connectionState)
                return;
            ConnectionState = connectionState;
            OnConnectionStateChange?.Invoke(this, connectionState);
        }

        void HandleConsumer(Consumer consumer)
        {
            consumer.OnGetStatsAsync += Consumer_OnGetStatsAsync;
        }

        async Task<IRTCStatsReport> Consumer_OnGetStatsAsync(object sender, string localId)
        {
            return await Handler.GetReceiverStatsAsync(localId);
        }

        void HandleProducer(Producer producer)
        {
            producer.OnGetStatsAsync += Producer_OnGetStatsAsync;

        }

        async Task<IRTCStatsReport> Producer_OnGetStatsAsync(object sender, string localId)
        {
            return await Handler.GetSenderStatsAsync(localId);
        }



    }

}
