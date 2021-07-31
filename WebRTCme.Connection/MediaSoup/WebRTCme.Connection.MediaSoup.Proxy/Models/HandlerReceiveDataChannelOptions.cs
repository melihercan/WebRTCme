using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class HandlerReceiveDataChannelOptions
    {
        public SctpStreamParameters SctpStreamParameters { get; init; }
        public string Label { get; init; }
        public string Protocol { get; init; }
    }
}
