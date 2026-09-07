using System.Text.Json;

namespace WebRTCme.Windows;

/// <summary>
/// Turns the JSON the shim hands back into an <see cref="IRTCStatsReport"/>.
/// </summary>
/// <remarks>
/// <para>
/// The payload is what <c>RTCStatsReport::ToJson</c> produces: an array of stats objects, each
/// opening with <c>type</c>, <c>id</c> and <c>timestamp</c> and then carrying whichever attributes
/// of its type have values. Absent attributes are omitted rather than written as null.
/// </para>
/// <para>
/// This is a different shape from the Blazor binding's, which receives a map already keyed by id,
/// so the two conversions stay separate rather than sharing a parser that fits neither.
/// </para>
/// </remarks>
internal static class StatsJson
{
    public static IRTCStatsReport ToStatsReport(string json)
    {
        var stats = new Dictionary<string, RTCStats>();

        // An empty report reaches us as "[]" because the shim substitutes one; ToJson itself
        // returns an empty string, which is not JSON at all.
        if (string.IsNullOrWhiteSpace(json))
            return new RTCStatsReport(stats);

        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            return new RTCStatsReport(stats);

        foreach (var entry in document.RootElement.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object)
                continue;

            var members = new Dictionary<string, object>();
            foreach (var member in entry.EnumerateObject())
            {
                if (member.NameEquals("id") || member.NameEquals("type") ||
                    member.NameEquals("timestamp"))
                    continue;

                members[member.Name] = ToValue(member.Value);
            }

            var id = ReadString(entry, "id");
            if (id is null)
                continue;

            stats[id] = new RTCStats
            {
                Id = id,
                Type = ReadString(entry, "type"),
                // WebRTC serialises Timestamp::us(); DOMHighResTimeStamp is milliseconds.
                Timestamp = ReadDouble(entry, "timestamp") / 1000d,
                Members = members
            };
        }

        return new RTCStatsReport(stats);
    }

    private static string ReadString(JsonElement entry, string name) =>
        entry.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static double ReadDouble(JsonElement entry, string name) =>
        entry.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : 0d;

    private static object ToValue(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            // Every 64-bit counter has already been through %.16g on the native side, so it
            // arrives as a double whether or not it reads as an integer here.
            JsonValueKind.Number => value.TryGetInt64(out var number) ? number : (object)value.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            // Sequence and map attributes, which stay in their serialised form here as they do
            // in the Android and iOS bindings.
            _ => value.GetRawText()
        };
}
