using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Utilme.SdpTransform
{
    public enum Direction
    {
        [Display(Name = "sendrecv")]
        SendRecv,

        [Display(Name = "sendonly")]
        SendOnly,

        [Display(Name = "recvonly")]
        RecvOnly,

        [Display(Name = "inactive")]
        Inactive
    }
}
