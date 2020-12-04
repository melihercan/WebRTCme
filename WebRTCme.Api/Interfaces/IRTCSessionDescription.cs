using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCSessionDescription : INativeObject
    {
        RTCSdpType Type { get; }
        
        string Sdp { get; }

        string ToJson();

    }
}
