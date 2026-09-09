using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum ConnectionState
    {
        [EnumMember(Value = "new")]
        [Display(Name = "new")]
        New,

        [EnumMember(Value = "connecting")]
        [Display(Name = "connecting")]
        Connecting,

        [EnumMember(Value = "connected")]
        [Display(Name = "connected")]
        Connected,

        [EnumMember(Value = "failed")]
        [Display(Name = "failed")]
        Failed,

        [EnumMember(Value = "disconnected")]
        [Display(Name = "disconnected")]
        Disconnected,

        [EnumMember(Value = "closed")]
        [Display(Name = "closed")]
        Closed
    }
}
