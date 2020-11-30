using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WebRTCme
{
    public enum RTCSignallingState
    {
        Stable,

        [Description("have-local-offer")]
        HaveLocalOffer,

        [Description("have-remote-offer")]
        HaveeEmoteOffer,

        [Description("have-local-pranswer")]
        HaveLocalPranswer,

        [Description("have-remote-pranswer")]
        HaveRemotePranswer
    }
}
