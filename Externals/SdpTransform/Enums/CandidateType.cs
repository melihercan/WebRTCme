using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum CandidateType
    {
        [EnumMember(Value = "host")]
        [Display(Name="host")]
        Host,

        [EnumMember(Value = "srflx")]
        [Display(Name = "srflx")]
        Srflx,

        [EnumMember(Value = "prlfx")]
        [Display(Name = "prlfx")]
        Prflx,

        [EnumMember(Value = "relay")]
        [Display(Name = "relay")]
        Relay
    }
}
