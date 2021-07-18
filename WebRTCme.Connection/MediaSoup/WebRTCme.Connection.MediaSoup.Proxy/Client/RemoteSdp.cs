using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    class RemoteSdp
    {
        IceParameters _iceParameters;
        IceCandidate[] _iceCandidates;
        DtlsParameters _dtlsParameters;
        SctpParameters _sctpParameters;
        PlainRtpParameters _plainRtpParameters;
        bool? _planB;

        Sdp _sdpObject;
        public RemoteSdp(IceParameters iceParameters, IceCandidate[] iceCandidates, DtlsParameters dtlsParameters, 
            SctpParameters sctpParameters, PlainRtpParameters plainRtpParameters, bool? planB)
        {
            _iceParameters = iceParameters;
            _iceCandidates = iceCandidates;
            _dtlsParameters = dtlsParameters;
            _sctpParameters = sctpParameters;
            _plainRtpParameters = plainRtpParameters;
            _planB = planB;


            _sdpObject = new Sdp
            {
                Version = 0,
                Origin = new Origin
                {
                    UserName = "mediasoup-client",
                    SessionId = 10000,
                    SessionVersion = 0,
                    Nettype = "IN",
                    AddrType = "0.0.0.0"
                },
                SessionName = new byte[] { (byte)'-' },
                Timings = new List<TimingInfo>() { new TimingInfo { StartTime = 0, StopTime = 0 } },
                MediaDescriptions = new List<MediaDescription>()
            };

        }
    }
}
