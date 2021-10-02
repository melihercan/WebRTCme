using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum CandidateTransport
    {
        [EnumMember(Value ="udp")]
        [Display(Name = "udp")]
        Udp,

        [EnumMember(Value = "tcp")]
        [Display(Name = "tcp")]
        Tcp
    }
}
