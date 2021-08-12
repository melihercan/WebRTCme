using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilme.SdpTransform
{
    public enum BandwidthType
    {
        [Display(Name="AS")]
        ApplicationSpecific,

        [Display(Name = "CT")]
        ConferenceTotal,

        [Display(Name = "RS")]
        RtcpSender,

        [Display(Name = "RR")]
        RtcpReceiver,

        [Display(Name = "TIAS")]
        TransportIndependentMaximumBandwidth,
    }
}
