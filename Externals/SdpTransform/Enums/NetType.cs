using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum NetType
    {
        [EnumMember(Value = "IN")]
        [Display(Name="IN")]
        Internet
    }
}
