using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCCertificate : ApiBase, IRTCCertificate
    {

        public static IRTCCertificate Create() => new RTCCertificate();

        private RTCCertificate() { }

        public ulong Expires => throw new NotImplementedException();

    }
}
