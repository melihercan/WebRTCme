using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Transport
    {
        InternalDirection _direction;
        TransportOptions _options;
        Handler _handler;
        ExtendedRtpCapabilities _extendedRtpCapabilities;
        CanProduceByKind _canProduceByKind;
        bool _closed;
        ConnectionState _connectionState = ConnectionState.New;

        public Transport(InternalDirection direction, TransportOptions options, Handler handler, 
            ExtendedRtpCapabilities extendedRtpCapabilities, CanProduceByKind canProduceByKind)
        {
            _direction = direction;
            _options = options;
            _handler = handler;
            _extendedRtpCapabilities = extendedRtpCapabilities;
            _canProduceByKind = canProduceByKind;
        }


        public async Task<Consumer> Consume(ConsumerOptions options)
        {
            Console.WriteLine("Consume()");

            var rtpParameters = Utils.Clone<RtpParameters>(options.RtpParameters, null);

            if (_closed)
                throw new Exception("closed");
            else if (_direction != InternalDirection.Recv)
                throw new Exception("not a receiving Transport");
            else if (options.Kind != MediaKind.Audio && options.Kind != MediaKind.Video)
                throw new Exception($"invalid kind {options.Kind}");
            else if (_connectionState == ConnectionState.New)
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

        void HandleConsumer(Consumer consumer)
        {
            consumer.OnGetStatsAsync += Consumer_OnGetStatsAsync;
        }

        async Task<IRTCStatsReport> Consumer_OnGetStatsAsync(object sender, string localId)
        {
            return await _handler.GetReceiverStatsAsync(localId);
        }

        void HandleProducer(Producer producer)
        {
            producer.OnGetStatsAsync += Producer_OnGetStatsAsync;

        }

        async Task<IRTCStatsReport> Producer_OnGetStatsAsync(object sender, string localId)
        {
            return await _handler.GetSenderStatsAsync(localId);
        }
    }

}
