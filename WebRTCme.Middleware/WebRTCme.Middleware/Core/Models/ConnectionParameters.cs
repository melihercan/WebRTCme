using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection;

namespace WebRTCme.Middleware
{
    public class ConnectionParameters
    {
        public ConnectionType ConnectionType { get; set; } 
        public string Name { get; set; }
        public string Room { get; set; }
    }
}
