using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCCertificate
    {
        DateTime Expires { get; set; }
    }
}
