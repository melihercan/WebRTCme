using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware.MediaStreamProxies.Models
{
    class ProtooResponseBase
    {
        public bool Response { get; init; }
        public uint Id { get; init; }
        public bool Ok { get; init; }
    }

    class ProtooResponseOk : ProtooResponseBase
    {
        public object Data { get; set; }
    }

    class ProtooResponseError : ProtooResponseBase
    {
        public uint ErrorCode { get; init; }
        public string ErrorReason { get; init; }
    }

}
