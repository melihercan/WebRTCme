using System.Collections;
using System.Collections.Generic;

namespace WebRTCme.Connection.Models
{
    /// <summary>
    /// One stats report built from several.
    /// </summary>
    /// <remarks>
    /// A mediasoup peer is received through as many consumers as it produces tracks, each with
    /// its own report, while callers ask for stats a peer at a time. Merging by stats id gives
    /// them the single snapshot the API promises. Entries from different reports do not collide:
    /// the ids identify the underlying RTP streams, which are distinct per consumer.
    /// </remarks>
    sealed class AggregateStatsReport : IRTCStatsReport
    {
        readonly IReadOnlyDictionary<string, RTCStats> _stats;

        public AggregateStatsReport(IReadOnlyDictionary<string, RTCStats> stats) => _stats = stats;

        public RTCStats this[string key] => _stats[key];

        public IEnumerable<string> Keys => _stats.Keys;

        public IEnumerable<RTCStats> Values => _stats.Values;

        public int Count => _stats.Count;

        public bool ContainsKey(string key) => _stats.ContainsKey(key);

        public bool TryGetValue(string key, out RTCStats value) => _stats.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, RTCStats>> GetEnumerator() => _stats.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
