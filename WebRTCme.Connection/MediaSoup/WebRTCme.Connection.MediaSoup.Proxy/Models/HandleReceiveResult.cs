using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class HandleReceiveResult
    {
        public string LocalId { get; init; }
        public IMediaStreamTrack Track { get; init; }
        public IRTCRtpReceiver RtpReceiver { get; init; }

    }
}
