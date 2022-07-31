using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCCertificate : NativeBase<Webrtc.RtcCertificatePem>, IRTCCertificate
    {

        public static IRTCCertificate Create(Webrtc.RtcCertificatePem nativeCertificatePem) =>
            new RTCCertificate(nativeCertificatePem);

        public RTCCertificate(Webrtc.RtcCertificatePem nativeCertificatePem) : base(nativeCertificatePem)
        { }

        public ulong Expires => throw new NotImplementedException();

    }
}