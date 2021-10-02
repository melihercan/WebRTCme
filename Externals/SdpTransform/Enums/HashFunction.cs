using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum HashFunction
    {
        [EnumMember(Value = "sha-1")]
        [Display(Name = "sha-1")]
        Sha1,

        [EnumMember(Value = "sha-224")]
        [Display(Name = "sha-224")]
        Sha224,

        [EnumMember(Value = "sha-256")]
        [Display(Name = "sha-256")]
        Sha256,

        [EnumMember(Value = "sha-384")]
        [Display(Name = "sha-384")]
        Sha384,

        [EnumMember(Value = "sha-512")]
        [Display(Name = "sha-512")]
        Sha512,

        [EnumMember(Value = "md2")]
        [Display(Name = "md2")]
        Md2,

        [EnumMember(Value = "md5")]
        [Display(Name = "md5")]
        Md5
    }
}
