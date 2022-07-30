using Webrtc = Org.Webrtc;
using System;
using WebRTCme;

namespace WebRTCme.Android
{
    internal class RTCCertificate : ApiBase, IRTCCertificate
    {

        public static IRTCCertificate Create(Webrtc.RtcCertificatePem nativeCertificatePem) =>
            new RTCCertificate(nativeCertificatePem);

        public RTCCertificate(Webrtc.RtcCertificatePem nativeCertificatePem) : base(nativeCertificatePem)
        { }

        public ulong Expires => throw new NotImplementedException();

    }
}