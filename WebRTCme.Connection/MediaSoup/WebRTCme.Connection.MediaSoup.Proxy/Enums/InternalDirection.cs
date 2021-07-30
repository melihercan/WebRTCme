using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum InternalDirection
    {
        [Display(Name = "send")]
        Send,

        [Display(Name = "recv")]
        Recv
    }
}
