using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum DtlsRole
    {
        [EnumMember(Value = "auto")]
        [Display(Name = "auto")]
        Auto,

        [EnumMember(Value = "client")]
        [Display(Name = "client")]
        Client,

        [EnumMember(Value = "server")]
        [Display(Name = "server")]
        Server
    }
}

