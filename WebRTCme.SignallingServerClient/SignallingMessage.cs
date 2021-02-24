using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.SignallingServerClient
{
    //// Currently copied from working example to be compatible with it.
    internal class SignallingMessage
    {
        public string Type { get; set; }
        public string Sdp { get; set; }
        public Candidate Candidate { get; set; }

    }
    internal class Candidate
    {
        public string Sdp { get; set; }
        public int SdpMLineIndex { get; set; }
        public string SdpMid { get; set; }
    }

    internal class Data
    {
        public string TurnServerName { get; set; }
        public string RoomName { get; set; }
        public string Name { get; set; }
    }

}
