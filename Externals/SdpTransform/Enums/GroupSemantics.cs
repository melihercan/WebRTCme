using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilme.SdpTransform
{
    public enum GroupSemantics
    {
        [Display(Name ="LS")]
        LipSynchronization,

        [Display(Name = "FID")]
        FlowIdentification,

        [Display(Name = "BUNDLE")]
        Bundle
    }
}
