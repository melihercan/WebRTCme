using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Transport
    {
        InternalDirection _direction;
        TransportOptions _options;
        Handler _handler;
        ExtendedRtpCapabilities _extendedRtpCapabilities;
        CanProduceByKind _canProduceByKind;

        public Transport(InternalDirection direction, TransportOptions options, Handler handler, 
            ExtendedRtpCapabilities extendedRtpCapabilities, CanProduceByKind canProduceByKind)
        {
            _direction = direction;
            _options = options;
            _handler = handler;
            _extendedRtpCapabilities = extendedRtpCapabilities;
            _canProduceByKind = canProduceByKind;
        }
    }
}
