using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCCertificate : NativeBase<Webrtc.RTCCertificate>, IRTCCertificate
    {

        public static IRTCCertificate Create(Webrtc.RTCCertificate nativeCertificate) => 
            new RTCCertificate(nativeCertificate);

        public RTCCertificate(Webrtc.RTCCertificate nativeCertificate) : base(nativeCertificate)
        {
        }

        public ulong Expires => throw new NotImplementedException();

    }
}
