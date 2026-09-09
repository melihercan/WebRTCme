namespace WebRTCme.Connection.MediaSoup
{
    public class SctpStreamParameters
    {
        public int? StreamId { get; init; }
        public bool? Ordered { get; set; }
        public int? MaxPacketLifeTime { get; init; }
        public int? MaxRetransmits { get; init; }
        public string Label { get; init; }
        public string Protocol { get; init; }
    }
}