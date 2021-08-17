using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilme.SdpTransform
{
    public enum SetupRole
    {
        [Display(Name = "active")]
        Active,

        [Display(Name = "passive")]
        Passive,

        [Display(Name = "actpass")]
        ActPass,

        [Display(Name = "holdconn")]
        HoldConn
    }
}
