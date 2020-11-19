using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WebRTCme
{
    public enum RTCBundlePolicy
    {
        Balanced,
        
        [Description("max-compat")]
        MaxCompat,
        
        [Description("max-bundle")]
        MaxBundle,
    }
}
