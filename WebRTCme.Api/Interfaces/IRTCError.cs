using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCError : IDOMException
    {
        RTCErrorDetailType ErrorDetail { get; }

        int? SdpLineNumber { get; }

        int? SctpCauseCode { get; }

        uint? ReceivedAlert { get; }

        uint? SentAlert { get; }
    }
}
