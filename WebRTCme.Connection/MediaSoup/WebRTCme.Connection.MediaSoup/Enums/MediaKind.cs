using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum MediaKind
    {
        [EnumMember(Value = "audio")]
        [Display(Name = "audio")]
        Audio,

        [EnumMember(Value = "video")]
        [Display(Name = "video")]
        Video,

        [EnumMember(Value = "application")]
        [Display(Name = "application")]
        Application

            ///
    }
}
