using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class GetProducerStatsResponse
    {
		public uint Bitrate { get; init; }
		public ulong ByteCount { get; init; }
		public uint FirCount { get; init; }
		public uint FractionLost { get; init; }
		public uint Jitter { get; init; }
		public string Kind { get; init; }
		public string MimeType { get; init; }
		public uint NackCount { get; init; }
		public uint NackPacketCount { get; init; }
		public uint PacketCount { get; init; }
		public uint PacketsDiscarded { get; init; }
		public uint PacketsLost { get; init; }
		public uint PacketsRepaired { get; init; }
		public uint PacketsRetransmitted { get; init; }
		public uint PliCount { get; init; }
		public double RoundTripTime { get; init; }
		public uint Score { get; init; }
		public uint Ssrc { get; init; }
		public uint Timestamp { get; init; }
		public string Type { get; init; }




		////public uint RtxSsrc { get; init; }
		////public string Rid { get; init; }
		////public uint RtxPacketsDiscarded { get; init; }
		////public object BitrateByLayer { get; init; }
	}
}
