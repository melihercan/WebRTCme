using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class MsidSemantic
    {
        public const string Label = "msid-semantic:";

        public string Token { get; init; }
        public const string WebRtcMediaStreamToken = "WMS";

        public string[] IdList { get; init; }
        public const string AllIds = "*";


    }
}
