using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilme.SdpTransform
{
    public enum EncryptionKeyMethod
    {
        [Display(Name= "clear")]
        Clear,

        [Display(Name = "base64")]
        Base64,

        [Display(Name = "uri")]
        Uri,

        [Display(Name = "prompt")]
        Prompt
    }
}
