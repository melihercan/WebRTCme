using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]

    public enum ConsumerType
    {
        [EnumMember(Value = "simple")]
        [Display(Name = "simple")]
        Simple,

        [EnumMember(Value = "simulcast")]
        [Display(Name = "simulcast")]
        Simulcast,

        [EnumMember(Value = "svc")]
        [Display(Name = "svc")]
        Svc,

        [EnumMember(Value = "pipe")]
        [Display(Name = "pipe")]
        Pipe
    }
}
