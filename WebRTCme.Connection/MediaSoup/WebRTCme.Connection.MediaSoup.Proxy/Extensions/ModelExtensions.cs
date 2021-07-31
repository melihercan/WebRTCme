using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
