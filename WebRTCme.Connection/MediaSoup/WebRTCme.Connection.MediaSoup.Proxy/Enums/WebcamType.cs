using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum WebcamType
    {
        [EnumMember(Value = "back")]
        [Display(Name = "back")]
        Back,

        [EnumMember(Value = "front")]
        [Display(Name = "front")]
        Front,

        [EnumMember(Value = "share")]
        [Display(Name = "share")]
        Share
    }
}
