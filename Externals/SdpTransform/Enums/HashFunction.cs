using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilme.SdpTransform
{
    public enum HashFunction
    {
        [Display(Name = "sha-1")]
        Sha1,

        [Display(Name = "sha-224")]
        Sha224,
        
        [Display(Name = "sha-256")]
        Sha256,
        
        [Display(Name = "sha-384")]
        Sha384,
        
        [Display(Name = "sha-512")]
        Sha512,
        
        [Display(Name = "md2")]
        Md2,

        [Display(Name = "md5")]
        Md5
    }
}
