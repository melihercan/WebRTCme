using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Shared.SipSorcery.Custom;

namespace WebRTCme.Shared.SipSorcery
{
    internal class RTCPeerConnectionIceEvent : NativeBase<SIPSorcery.Net.RTCIceCandidate>, IRTCPeerConnectionIceEvent
    {
        private readonly SIPSorcery.Net.RTCIceCandidate _nativeIceCandidate;

        public RTCPeerConnectionIceEvent(SIPSorcery.Net.RTCIceCandidate nativeIceCandidate)
        {
            _nativeIceCandidate = nativeIceCandidate;
        }

        public IRTCIceCandidate Candidate =>
            new RTCIceCandidate(_nativeIceCandidate);

        public void Dispose()
        {
        }
    }
}
