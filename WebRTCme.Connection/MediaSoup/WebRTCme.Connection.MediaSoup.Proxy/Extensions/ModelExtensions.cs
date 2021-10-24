using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace WebRTCme.Connection.MediaSoup.Proxy
{
    public static class ModelExtensions
    {
        public static RTCRtpEncodingParameters ToWebRtc(this RtpEncodingParameters parameters)
        {
            RTCRtpEncodingParameters params_ = new()
            {
                //Ptime = parameters.
                Rid = parameters.Rid,
            };


            if (parameters.CodecPayloadType.HasValue)
                params_.CodecPayloadType = (byte)parameters.CodecPayloadType;
            if (parameters.Dtx.HasValue)
                params_.Dtx = (bool)parameters.Dtx ? RTCDtxStatus.Enabled : RTCDtxStatus.Disabled;
            if (parameters.MaxBitrate.HasValue)
                params_.MaxBitrate = (ulong)parameters.MaxBitrate;
            if (parameters.MaxFramerate.HasValue)
                params_.MaxFramerate = Convert.ToDouble(parameters.MaxFramerate);
            if (parameters.ScaleResolutionDownBy.HasValue)
                params_.ScaleResolutionDownBy = Convert.ToDouble(parameters.ScaleResolutionDownBy);

            return params_;
        }

        // Helper to convert object to either string or number after JSON conversion.
        public static void ToStringOrNumber(this Dictionary<string, object> dictionary)
        {
            Dictionary<string, object> cloneDictionary = new(dictionary);
            foreach (var item in cloneDictionary)
            {
                if (item.Value.GetType() == typeof(JsonElement))
                {
                    var jElement = (JsonElement)item.Value;
                    if (jElement.ValueKind == JsonValueKind.String)
                        dictionary[item.Key] = jElement.GetString();
                    else if (jElement.ValueKind == JsonValueKind.Number)
                        dictionary[item.Key] = jElement.GetInt32();
                    else
                        throw new NotSupportedException();
                }
            }
        }

        // Helper to convert object to string, number or bool after JSON conversion.
        public static void ToStringOrNumberOrBool(this Dictionary<string, object> dictionary)
        {
            Dictionary<string, object> cloneDictionary = new(dictionary);
            foreach (var item in cloneDictionary)
            {
                if (item.Value.GetType() == typeof(JsonElement))
                {
                    var jElement = (JsonElement)item.Value;
                    if (jElement.ValueKind == JsonValueKind.String)
                        dictionary[item.Key] = jElement.GetString();
                    else if (jElement.ValueKind == JsonValueKind.Number)
                        dictionary[item.Key] = jElement.GetInt32();
                    else if (jElement.ValueKind == JsonValueKind.False || jElement.ValueKind == JsonValueKind.True)
                        dictionary[item.Key] = jElement.GetBoolean();
                    else
                        throw new NotSupportedException();
                }
            }
        }

    }
}
