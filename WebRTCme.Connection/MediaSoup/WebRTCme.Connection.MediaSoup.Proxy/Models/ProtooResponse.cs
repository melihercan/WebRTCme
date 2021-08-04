using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class ProtooResponseBase
    {
        public bool Response { get; init; }
        public uint Id { get; init; }
        public bool Ok { get; init; }
    }

    public class ProtooResponseOk : ProtooResponseBase
    {
        public object Data { get; set; }
    }

    public class ProtooResponseError : ProtooResponseBase
    {
        public int ErrorCode { get; init; }
        public string ErrorReason { get; init; }
    }

    public class ProtooResponse : ProtooResponseBase
    {
        public object Data { get; set; }
        public int? ErrorCode { get; init; }
        public string ErrorReason { get; init; }

    }
}


