using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCCertificate : ApiBase, IRTCCertificate
    {

        public static IRTCCertificate Create(Webrtc.RTCCertificate nativeCertificate) => 
            new RTCCertificate(nativeCertificate);

        private RTCCertificate(Webrtc.RTCCertificate nativeCertificate) : base(nativeCertificate)
        {
        }

        public ulong Expires => throw new NotImplementedException();

    }
}
