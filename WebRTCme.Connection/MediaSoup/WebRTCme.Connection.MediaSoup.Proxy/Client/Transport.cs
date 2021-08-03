using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Transport
    {
        readonly Ortc _ortc;
        TransportOptions _options;
        ExtendedRtpCapabilities _extendedRtpCapabilities;
        CanProduceByKind _canProduceByKind;
        int? _maxSctpMessageSize;
        bool _probatorConsumerCreated;

        Dictionary<string, Producer> _producers = new();
        Dictionary<string, Consumer> _consumers = new();
        Dictionary<string, DataProducer> _dataProducers = new();
        Dictionary<string, DataConsumer> _dataConsumers = new();

        //// TODO: TYPESCRIPT code uses AwaitQueue to execute commands, is it required???

        public string Id { get; }
        public bool Closed { get; private set; }

        public InternalDirection Direction { get; }

        public Handler Handler { get; }

        public ConnectionState ConnectionState { get; private set; }
        public object AppData { get; }

        public event EventHandler<ConnectionState> OnConnectionStateChange;
        public event EventHandlerAsync<DtlsParameters> OnConnectAsync;
        public event EventHandlerAsync<ProduceEventParameters, string> OnProduceAsync;

        public Transport(Ortc ortc, InternalDirection direction, TransportOptions options, Handler handler,
            ExtendedRtpCapabilities extendedRtpCapabilities, CanProduceByKind canProduceByKind)
        {
            _ortc = ortc;
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

        public void Close()
        {
            if (Closed)
                return;

            Closed = true;

            Handler.Close();

            // Close all Producers.
            foreach (var producer in _producers.Values)
		    {
                producer.TransportClosed();
            }
            _producers.Clear();

            // Close all Consumers.
            foreach (var consumer in _consumers.Values)
		    {
                consumer.TransportClosed();
            }
            this._consumers.Clear();

            // Close all DataProducers.
            foreach (var dataProducer in _dataProducers.Values)
		    {
                dataProducer.TransportClosed();
            }
            _dataProducers.Clear();

            // Close all DataConsumers.
            foreach (var dataConsumer in _dataConsumers.Values)
		    {
                dataConsumer.TransportClosed();
            }
            _dataConsumers.Clear();
        }

        public Task<IRTCStatsReport> GetStatsAsync()
        {
            if (Closed)
                throw new Exception("Closed");

            return Handler.GetTransportStatsAsync();
        }

        public Task RestartIceAsync(IceParameters iceParameters)
        {
            if (Closed)
                throw new Exception("Closed");

            return Handler.RestartIceAsync(iceParameters);
        }

        public Task UpdateIceServersAsync(RTCIceServer[] iceServers)
        {
            if (Closed)
                throw new Exception("Closed");

            return Handler.UpdateIceServersAsync(iceServers);
        }

        public async Task<Producer> ProduceAsync(ProducerOptions options)
        {
            Console.WriteLine($"produce() track:{options.Track}");

            if (options.Track is null)
                throw new Exception("missing track");
            else if (Direction != InternalDirection.Send)
                throw new Exception("not a sending Transport");
            else if (!_canProduceByKind[options.Track.Kind.ToMediaSoup()])
                throw new Exception($"cannot produce {options.Track.Kind}");
            else if (options.Track.ReadyState ==  MediaStreamTrackState.Ended)
                throw new Exception("track ended");
            else if (ConnectionState == ConnectionState.New)
                throw new Exception("no connect listener set into this transport");


            try
            {
                RtpEncodingParameters[] normalizedEncodings = null;
                if (options.Encodings is null)
                {
                    throw new Exception("encodings must be an array");
                }
                else if (options.Encodings.Length == 0)
                {
                    normalizedEncodings = null;
                }
                else
                {
                    normalizedEncodings = options.Encodings.Select(encoding =>
                    {
                        var normalizedEncoding = new RtpEncodingParameters 
                        { 
                            Active = encoding.Active,
                            Dtx = encoding.Dtx,
                            ScalabilityMode = encoding.ScalabilityMode,
                            ScaleResolutionDownBy = encoding.ScaleResolutionDownBy,
                            MaxBitrate = encoding.MaxBitrate,
                            MaxFramerate = encoding.MaxFramerate,
                            AdaptivePtime = encoding.AdaptivePtime,
                            Priority = encoding.Priority,
                            NetworkPriority = encoding.NetworkPriority
                        };
                        return normalizedEncoding;
                    }).ToArray();
                }

                var handlerSendResult = await Handler.SendAsync(new HandlerSendOptions
                {
                    Track = options.Track,
					Encodings = normalizedEncodings,
					CodecOptions = options.CodecOptions,
					Codec = options.Codec
                });

                try
                {
                    // This will fill rtpParameters's missing fields with default values.
                    _ortc.ValidateRtpParameters(handlerSendResult.RtpParameters);

                    var id = await OnProduceAsync(this, new ProduceEventParameters 
                    { 
                        Kind = options.Track.Kind.ToMediaSoup(),
                        RtpParameters = handlerSendResult.RtpParameters,
                        AppData = options.AppData
                    });

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
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }

 return null;
        }

        public async Task<Consumer> ConsumeAsync(ConsumerOptions options)
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
