using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public enum IpVersion
    {
        [Display(Name="4")]
        Ip4,

        [Display(Name = "6")]
        Ip6
    }
}
