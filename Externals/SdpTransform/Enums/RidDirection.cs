using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum RidDirection
    {
        [EnumMember(Value = "recv")]
        [Display(Name = "recv")]
        Recv,

        [EnumMember(Value = "send")]
        [Display(Name = "send")]
        Send,
    }
}
