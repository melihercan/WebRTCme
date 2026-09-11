using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace WebRTCme.Connection.MediaSoup
{
    public static class ModelExtensions
    {
        public static RTCRtpEncodingParameters ToWebRtc(this RtpEncodingParameters parameters)
        {
            RTCRtpEncodingParameters params_ = new()
            {
                Rid = parameters.Rid,
            };


            if (parameters.MaxBitrate.HasValue)
                params_.MaxBitrate = (ulong)parameters.MaxBitrate;
            if (parameters.MaxFramerate.HasValue)
                params_.MaxFramerate = Convert.ToDouble(parameters.MaxFramerate);
            if (parameters.ScaleResolutionDownBy.HasValue)
                params_.ScaleResolutionDownBy = Convert.ToDouble(parameters.ScaleResolutionDownBy);

            return params_;
        }

        /// <summary>
        /// Replaces the <see cref="JsonElement"/>s left by deserialisation with the strings and numbers
        /// mediasoup expects, returning a new dictionary.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These dictionaries are <c>Dictionary&lt;string, object&gt;</c> because the wire format genuinely
        /// is heterogeneous - codec parameters and appData carry whatever the server put there. So
        /// <c>System.Text.Json</c> hands back <see cref="JsonElement"/> values that nothing downstream
        /// understands, and they have to be coerced once, on arrival.
        /// </para>
        /// <para>
        /// This used to edit the caller's dictionary in place and return nothing, which made the conversion
        /// invisible at the call site and left no way to tell a converted dictionary from an untouched one.
        /// Returning a new one makes the caller say so: <c>x = x.ToStringOrNumber()</c>.
        /// </para>
        /// </remarks>
        public static Dictionary<string, object> ToStringOrNumber(this Dictionary<string, object> dictionary) =>
            ConvertValues(dictionary, allowBool: false);

        /// <summary>As <see cref="ToStringOrNumber"/>, and booleans are permitted too.</summary>
        public static Dictionary<string, object> ToStringOrNumberOrBool(this Dictionary<string, object> dictionary) =>
            ConvertValues(dictionary, allowBool: true);

        static Dictionary<string, object> ConvertValues(Dictionary<string, object> dictionary, bool allowBool)
        {
            if (dictionary is null)
                return null;

            var converted = new Dictionary<string, object>(dictionary.Count);

            foreach (var item in dictionary)
            {
                // Anything already converted, or never a JsonElement to begin with, passes through
                // untouched. Note this also covers a null value, which is legitimate JSON and which the
                // previous version dereferenced.
                if (item.Value is not JsonElement element)
                {
                    converted[item.Key] = item.Value;
                    continue;
                }

                converted[item.Key] = element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),

                    // Not every JSON number is an int: a codec parameter can legitimately be fractional,
                    // and GetInt32 throws on those rather than rounding.
                    JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),

                    JsonValueKind.True or JsonValueKind.False when allowBool => element.GetBoolean(),

                    JsonValueKind.Null => null,

                    // Named rather than bare: an unexpected shape means the server sent something this
                    // client does not model, and the key is what identifies it.
                    _ => throw new NotSupportedException(
                        $"'{item.Key}' is a JSON {element.ValueKind}, which cannot be converted to " +
                        $"{(allowBool ? "a string, number or bool" : "a string or number")}.")
                };
            }

            return converted;
        }

    }
}
