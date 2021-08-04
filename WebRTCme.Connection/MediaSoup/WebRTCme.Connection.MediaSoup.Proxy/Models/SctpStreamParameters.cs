﻿namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class SctpStreamParameters
    {
        public int? StreamId { get; init; }
        public bool? Ordered { get; init; }
        public int? MaxPacketLifeTime { get; init; }
        public int? MaxRetransmits { get; init; }
        public string Label { get; init; }
        public string Protocol { get; init; }
    }
}