using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal static class StatsExtensions
    {
        /// <summary>
        /// Completes a stats task from the SDK's fire-and-forget completion handler.
        /// </summary>
        public static void Complete(this TaskCompletionSource<IRTCStatsReport> tcs,
            Webrtc.RTCStatisticsReport nativeReport)
        {
            // This runs on a native thread. Letting an exception escape would take down the
            // process and leave the awaiting caller hanging, so surface it on the task instead.
            try
            {
                tcs.TrySetResult(nativeReport.FromNative());
            }
            catch (Exception exception)
            {
                tcs.TrySetException(exception);
            }
        }

        public static IRTCStatsReport FromNative(this Webrtc.RTCStatisticsReport nativeReport)
        {
            var stats = new Dictionary<string, RTCStats>();
            var nativeStatistics = nativeReport?.Statistics;

            if (nativeStatistics is not null)
            {
                foreach (var key in nativeStatistics.Keys)
                {
                    var nativeStats = nativeStatistics[key];
                    if (nativeStats is null)
                        continue;

                    stats[key.ToString()] = new RTCStats
                    {
                        Id = nativeStats.Id,
                        Type = nativeStats.Type,
                        // The SDK reports microseconds; DOMHighResTimeStamp is milliseconds.
                        Timestamp = nativeStats.Timestamp_us / 1000d,
                        Members = FromNativeValues(nativeStats.Values)
                    };
                }
            }

            return new RTCStatsReport(stats);
        }

        private static Dictionary<string, object> FromNativeValues(NSDictionary<NSString, NSObject> nativeValues)
        {
            var members = new Dictionary<string, object>();
            if (nativeValues is null)
                return members;

            foreach (var key in nativeValues.Keys)
                members[key.ToString()] = FromNativeValue(nativeValues[key]);

            return members;
        }

        private static object FromNativeValue(NSObject value)
        {
            switch (value)
            {
                case null:
                    return null;
                case NSNumber number:
                    return FromNativeNumber(number);
                case NSString text:
                    return text.ToString();
                case NSArray array:
                    var items = new object[(int)array.Count];
                    for (nuint index = 0; index < array.Count; index++)
                        items[index] = FromNativeValue(array.GetItem<NSObject>(index));
                    return items;
            }

            // The map-valued members have no closer .NET equivalent.
            return value.ToString();
        }

        private static object FromNativeNumber(NSNumber number) =>
            number.ObjCType switch
            {
                // Objective-C encodes BOOL as a char.
                "c" or "B" => number.BoolValue,
                "f" or "d" => number.DoubleValue,
                _ => number.Int64Value
            };
    }
}
