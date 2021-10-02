using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum AddrType
    {
        [EnumMember(Value = "IP4")]
        [Display(Name = "IP4")]
        Ip4,

        [EnumMember(Value = "IP6")]
        [Display(Name = "IP6")]
        Ip6

    }
}
