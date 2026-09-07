using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Platforms.Android.Custom
{
    internal static class StatsExtensions
    {
        /// <summary>
        /// Bridges the SDK's collector callback, which is fire-and-forget, to a Task.
        /// </summary>
        internal sealed class StatsCollectorProxy : Java.Lang.Object, Webrtc.IRTCStatsCollectorCallback
        {
            private readonly TaskCompletionSource<IRTCStatsReport> _tcs;

            public StatsCollectorProxy(TaskCompletionSource<IRTCStatsReport> tcs) => _tcs = tcs;

            public void OnStatsDelivered(Webrtc.RTCStatsReport report)
            {
                // This runs on a native thread. Letting an exception escape would take down the
                // process and leave the awaiting caller hanging, so surface it on the task instead.
                try
                {
                    _tcs.TrySetResult(report.FromNative());
                }
                catch (Exception exception)
                {
                    _tcs.TrySetException(exception);
                }
            }
        }

        public static IRTCStatsReport FromNative(this Webrtc.RTCStatsReport nativeReport)
        {
            var stats = new Dictionary<string, RTCStats>();

            foreach (var entry in nativeReport?.StatsMap ?? new Dictionary<string, Webrtc.RTCStats>())
            {
                var nativeStats = entry.Value;
                stats[entry.Key] = new RTCStats
                {
                    Id = nativeStats.Id,
                    Type = nativeStats.Type,
                    // The SDK reports microseconds; DOMHighResTimeStamp is milliseconds.
                    Timestamp = nativeStats.TimestampUs / 1000d,
                    Members = (nativeStats.Members ?? new Dictionary<string, Java.Lang.Object>())
                        .ToDictionary(member => member.Key, member => FromNativeMember(member.Value))
                };
            }

            return new RTCStatsReport(stats);
        }

        private static object FromNativeMember(Java.Lang.Object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case Java.Lang.Boolean nativeBoolean:
                    return nativeBoolean.BooleanValue();
                case Java.Lang.Integer nativeInteger:
                    return nativeInteger.IntValue();
                case Java.Lang.Long nativeLong:
                    return nativeLong.LongValue();
                case Java.Lang.Double nativeDouble:
                    return nativeDouble.DoubleValue();
                case Java.Lang.Float nativeFloat:
                    return nativeFloat.FloatValue();
                case Java.Lang.String nativeString:
                    return nativeString.ToString();
            }

            // Stats members are boxed, so an array member is an array of boxed values.
            if (value.Class is not null && value.Class.IsArray)
                return JNIEnv.GetArray<Java.Lang.Object>(value.Handle)
                    ?.Select(FromNativeMember)
                    .ToArray();

            // BigInteger and the map-valued members have no closer .NET equivalent.
            return value.ToString();
        }
    }
}
