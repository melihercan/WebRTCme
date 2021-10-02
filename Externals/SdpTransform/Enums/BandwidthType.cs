using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum BandwidthType
    {
        [EnumMember(Value = "")]
        [Display(Name="AS")]
        ApplicationSpecific,

        [EnumMember(Value = "CT")]
        [Display(Name = "CT")]
        ConferenceTotal,

        [EnumMember(Value = "RS")]
        [Display(Name = "RS")]
        RtcpSender,

        [EnumMember(Value = "RR")]
        [Display(Name = "RR")]
        RtcpReceiver,

        [EnumMember(Value = "TIAS")]
        [Display(Name = "TIAS")]
        TransportIndependentMaximumBandwidth,
    }
}
