using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
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
