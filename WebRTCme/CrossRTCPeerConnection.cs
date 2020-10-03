using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public static class CrossRTCPeerConnection
    {
        private static Lazy<IRTCPeerConnection> rtcPeerConnection =
            new Lazy<IRTCPeerConnection>(() => CreateRtcPeerConnection());

        public static IRTCPeerConnection Create()
        {
//            get
            {
                IRTCPeerConnection ret = rtcPeerConnection.Value;
                return ret;
            }
        }

        private static IRTCPeerConnection CreateRtcPeerConnection()
        {
//#if NETSTANDARD2_0
  //          return null;
//#else

            //// TODO: POTENTIAL PROBLEM HERE. For example: Android in this lib is 10.0. If client is in different
            /// version, .net standard DLL will be called here!!! Look for a solution.
            return new RTCPeerConnection();
//#endif
        }
    }
}
