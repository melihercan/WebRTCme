using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum SetupRole
    {
        [EnumMember(Value = "active")]
        [Display(Name = "active")]
        Active,

        [EnumMember(Value = "passive")]
        [Display(Name = "passive")]
        Passive,

        [EnumMember(Value = "actpass")]
        [Display(Name = "actpass")]
        ActPass,

        [EnumMember(Value = "holdconn")]
        [Display(Name = "holdconn")]
        HoldConn
    }
}
