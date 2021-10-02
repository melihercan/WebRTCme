using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum Direction
    {
        [EnumMember(Value = "sendrecv")]
        [Display(Name = "sendrecv")]
        SendRecv,

        [EnumMember(Value = "sendonly")]
        [Display(Name = "sendonly")]
        SendOnly,

        [EnumMember(Value = "recvonly")]
        [Display(Name = "recvonly")]
        RecvOnly,

        [EnumMember(Value = "inactive")]
        [Display(Name = "inactive")]
        Inactive
    }
}
