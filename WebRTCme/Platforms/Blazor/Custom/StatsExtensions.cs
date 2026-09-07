using System;
using System.Collections.Generic;
using System.Text.Json;
using WebRTCme;

namespace WebRTCme.Platforms.Blazor.Custom
{
    internal static class StatsExtensions
    {
        private static readonly string[] Standard = { "id", "type", "timestamp" };

        /// <summary>
        /// Turns the flattened RTCStatsReport the JS layer produces into the common report type.
        /// </summary>
        public static IRTCStatsReport ToStatsReport(
            this Dictionary<string, Dictionary<string, JsonElement>> report)
        {
            var stats = new Dictionary<string, RTCStats>();

            foreach (var entry in report ?? new Dictionary<string, Dictionary<string, JsonElement>>())
            {
                var members = new Dictionary<string, object>();
                foreach (var member in entry.Value)
                {
                    if (Array.IndexOf(Standard, member.Key) >= 0)
                        continue;
                    members[member.Key] = ToValue(member.Value);
                }

                stats[entry.Key] = new RTCStats
                {
                    Id = ReadString(entry.Value, "id") ?? entry.Key,
                    Type = ReadString(entry.Value, "type"),
                    Timestamp = ReadDouble(entry.Value, "timestamp"),
                    Members = members
                };
            }

            return new RTCStatsReport(stats);
        }

        private static string ReadString(Dictionary<string, JsonElement> members, string name) =>
            members.TryGetValue(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;

        private static double ReadDouble(Dictionary<string, JsonElement> members, string name) =>
            members.TryGetValue(name, out var value) && value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : 0d;

        private static object ToValue(JsonElement value) =>
            value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.TryGetInt64(out var number) ? number : value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => value.ToString()
            };
    }
}
