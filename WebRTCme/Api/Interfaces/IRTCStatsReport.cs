using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    /// <summary>
    /// A snapshot of connection statistics, keyed by stats id.
    /// </summary>
    /// <remarks>
    /// RTCStatsReport is a readonly maplike in the spec, so this is a read-only dictionary rather
    /// than a single stats object. Reports are snapshots: the values do not change after the call
    /// that produced them.
    /// </remarks>
    public interface IRTCStatsReport : IReadOnlyDictionary<string, RTCStats>
    {
    }
}
