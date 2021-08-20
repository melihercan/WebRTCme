using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum MediaKind
    {
        [Display(Name = "audio")]
        Audio,

        [Display(Name = "video")]
        Video,

        [Display(Name = "application")]
        Application
    }
}
