using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class ProtooNotification
    {
        public bool Notification { get; init; }
        public string Method { get; init; }
        public object Data { get; init; }

    }
}
