using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Enums
{
    public enum Priority
    {
        [Display(Name = "very-low")]
        VeryLow,

        [Display(Name = "low")]
        Low,

        [Display(Name = "medium")]
        Medium,

        [Display(Name = "high")]
        High
    }
}
