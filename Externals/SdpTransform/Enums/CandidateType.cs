using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    public enum CandidateType
    {
        [Display(Name="host")]
        Host,
        
        [Display(Name = "srflx")]
        Srflx,
        
        [Display(Name = "prlfx")]
        Prflx,
        
        [Display(Name = "relay")]
        Relay
    }
}
