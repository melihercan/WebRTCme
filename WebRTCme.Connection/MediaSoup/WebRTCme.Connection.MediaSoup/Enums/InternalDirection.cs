using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum InternalDirection
    {
        [EnumMember(Value="send")]
        [Display(Name = "send")]
        Send,

        [EnumMember(Value = "recv")]
        [Display(Name = "recv")]
        Recv
    }
}
