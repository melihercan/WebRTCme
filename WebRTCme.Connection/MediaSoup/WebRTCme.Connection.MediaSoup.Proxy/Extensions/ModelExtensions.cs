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
            return new RTCRtpEncodingParameters
            {
                CodecPayloadType = (byte)parameters.CodecPayloadType,
                Dtx = (bool)parameters.Dtx ? RTCDtxStatus.Enabled : RTCDtxStatus.Disabled,
                MaxBitrate = (ulong)parameters.MaxBitrate,
                MaxFramerate = Convert.ToDouble(parameters.MaxFramerate),
                //Ptime = parameters.
                Rid = parameters.Rid,
                ScaleResolutionDownBy = Convert.ToDouble(parameters.ScaleResolutionDownBy)
            };
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
                }
            }
        }
    }
}
