using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum MediaType
    {
        [EnumMember(Value = "audio")]
        [Display(Name = "audio")]
        Audio,

        [EnumMember(Value = "video")]
        [Display(Name = "video")]
        Video,

        [EnumMember(Value = "text")]
        [Display(Name = "text")]
        Text,

        [EnumMember(Value = "application")]
        [Display(Name = "application")]
        Application,

        [EnumMember(Value = "message")]
        [Display(Name = "message")]
        Message,
    }
}
