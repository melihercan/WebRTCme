using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class HandlerSendDataChannelResult
    {
        public IRTCDataChannel DataChannel { get; init; }
        public SctpStreamParameters SctpStreamParameters { get; init; }
    }
}
