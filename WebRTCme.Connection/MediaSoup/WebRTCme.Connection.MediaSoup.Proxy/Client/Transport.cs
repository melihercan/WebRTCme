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


        //public async Task<Consumer> Consume(ConsumerOptions options)
        //{
        //    Console.WriteLine("Consume()");

        //    var rtpParameters = Utils.Clone<RtpParameters>(options.RtpParameters, null);

        //    if (_closed)
        //        throw new Exception("closed");
        //    else if (_direction != InternalDirection.Recv)
        //        throw new Exception("not a receiving Transport");
        //    else if (options.Kind != MediaKind.Audio && options.Kind != MediaKind.Video)
        //        throw new Exception($"invalid kind {options.Kind}");
        //    else if (_connectionState == ConnectionState.New)
        //        throw new Exception($"no connect listener set into this transport");



        //}


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

        void HandleProducer(Producer producer)
        {
            producer.GetStatsEvent += Producer_GetStatsEvent;

        }

        private async Task<IRTCStatsReport> Producer_GetStatsEvent(string producerLocalId)
        {
            return await _handler.GetSenderStatsAsync(producerLocalId);
        }
    }

}
