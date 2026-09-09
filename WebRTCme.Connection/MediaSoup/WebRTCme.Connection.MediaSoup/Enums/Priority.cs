using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup.Proxy.Enums
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum Priority
    {
        [EnumMember(Value = "very-low")]
        [Display(Name = "very-low")]
        VeryLow,

        [EnumMember(Value = "low")]
        [Display(Name = "low")]
        Low,

        [EnumMember(Value = "medium")]
        [Display(Name = "medium")]
        Medium,

        [EnumMember(Value = "high")]
        [Display(Name = "high")]
        High
    }
}
