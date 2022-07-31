using System;
using System.Collections.Generic;

namespace WebRTCme
{
    public interface IRTCIceTransport : IDisposable // INativeObject
    {
        RTCIceComponent Component { get; }

        RTCIceGatheringState GatheringState { get; }

        RTCIceRole Role { get; }

        RTCIceTransportState State { get; }

        event EventHandler OnGatheringStateChange;
        event EventHandler OnSelectedCandidatePairChange;
        event EventHandler OnStateChange;

        IRTCIceCandidate[] GetLocalCandidates();

        RTCIceParameters GetLocalParameters();

        IRTCIceCandidate[] GetRemoteCandidates();

        RTCIceParameters GetRemoteParameters();

        IRTCIceCandidatePair GetSelectedCandidatePair();
    }
}