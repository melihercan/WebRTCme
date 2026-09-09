using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    /// <summary>
    /// The report a platform hands back from GetStats. Platform-neutral, so each binding only has to
    /// turn its native stats into <see cref="RTCStats"/> entries.
    /// </summary>
    public sealed class RTCStatsReport : IRTCStatsReport
    {
        private readonly IReadOnlyDictionary<string, RTCStats> _stats;

        public RTCStatsReport(IReadOnlyDictionary<string, RTCStats> stats) =>
            _stats = stats ?? new Dictionary<string, RTCStats>();

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
