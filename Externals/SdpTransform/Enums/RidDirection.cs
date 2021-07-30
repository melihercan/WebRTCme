using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilme.SdpTransform
{
    public enum RidDirection
    {
        [Display(Name = "recv")]
        Recv,

        [Display(Name = "send")]
        Send,
    }
}
