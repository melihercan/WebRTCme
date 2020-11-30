﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCSessionDescription
    {
        RTCSdpType Type { get; set; }
        
        string Sdp { get; set; }

    }
}
