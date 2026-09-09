using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    /// <summary>
    /// One entry of an <see cref="IRTCStatsReport"/>.
    /// </summary>
    /// <remarks>
    /// The spec gives every entry an id, a timestamp and a type, then lets the type decide the rest
    /// of the members. Those live in <see cref="Members"/>, keyed by their spec name, because the set
    /// is defined by the separate WebRTC Statistics specification and changes independently of this one.
    /// </remarks>
    public class RTCStats
    {
        public string Id { get; init; }

        public double Timestamp { get; init; }

        /// <summary>The stats type, e.g. "inbound-rtp" or "candidate-pair".</summary>
        public string Type { get; init; }

        public IReadOnlyDictionary<string, object> Members { get; init; } =
            new Dictionary<string, object>();
    }
}
