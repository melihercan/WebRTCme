using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilme.SdpTransform
{
    public enum MediaType
    {
        [Display(Name = "audio")]
        Audio,

        [Display(Name = "video")]
        Video,

        [Display(Name = "text")]
        Text,

        [Display(Name = "application")]
        Application,

        [Display(Name = "message")]
        Message,
    }
}
