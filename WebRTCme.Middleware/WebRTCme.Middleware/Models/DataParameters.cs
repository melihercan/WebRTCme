using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class DataParameters
    {
        public DataFromType From { get; set; }

        public string PeerUserName { get; set; }

        string PeerUserNameTextColor { get; set; } 

        public string Time { get; set; }

        public byte[] Bytes { get; set; }

        public string Message { get; set; }
    }
}
