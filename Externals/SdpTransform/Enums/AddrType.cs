using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilme.SdpTransform
{
    public enum AddrType
    {
        [Display(Name = "IP4")]
        Ip4,

        [Display(Name = "IP6")]
        Ip6

    }
}
