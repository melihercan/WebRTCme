using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class MethodName
    {
        public const string GetRouterRtpCapabilities = "getRouterRtpCapabilities";
        public const string CreateWebRtcTransport = "createWebRtcTransport";
        public const string Join = "join";
        public const string NewConsumer = "newConsumer";
        public const string NewDataConsumer = "newDataConsumer";
        public const string ConnectWebRtcTransport = "connectWebRtcTransport";
        public const string Produce = "produce";
        public const string NewPeer = "newPeer";
    }
}
