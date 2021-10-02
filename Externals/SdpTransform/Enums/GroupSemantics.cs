using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum GroupSemantics
    {
        [EnumMember(Value = "LS")]
        [Display(Name ="LS")]
        LipSynchronization,

        [EnumMember(Value = "FID")]
        [Display(Name = "FID")]
        FlowIdentification,

        [EnumMember(Value = "BUNDLE")]
        [Display(Name = "BUNDLE")]
        Bundle
    }
}
