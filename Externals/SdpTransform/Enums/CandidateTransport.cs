using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    public enum CandidateTransport
    {
        [Display(Name = "udp")]
        Udp,

        [Display(Name = "tcp")]
        Tcp
    }
}
