using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRTCme.Middleware.Models
{
    public class PeerContext
    {
        public PeerParameters PeerParameters { get; set; }
        
        public IRTCPeerConnection PeerConnection { get; set; }

        public bool IsInitiator { get; set; }

        public Subject<PeerResponseParameters> PeerResponseSubject { get; set; }

        public IDisposable PeerResponseDisposer { get; set; }
    }
}
