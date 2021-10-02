using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum EncryptionKeyMethod
    {
        [EnumMember(Value = "clear")]
        [Display(Name= "clear")]
        Clear,

        [EnumMember(Value = "base64")]
        [Display(Name = "base64")]
        Base64,

        [EnumMember(Value = "uri")]
        [Display(Name = "uri")]
        Uri,

        [EnumMember(Value = "prompt")]
        [Display(Name = "prompt")]
        Prompt
    }
}
