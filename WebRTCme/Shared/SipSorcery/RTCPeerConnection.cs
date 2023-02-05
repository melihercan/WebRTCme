using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Shared.SipSorcery.Custom;

namespace WebRTCme.Shared.SipSorcery;

internal class RTCPeerConnection : NativeBase<SIPSorcery.Net.RTCPeerConnection>, IRTCPeerConnection
{
    public RTCPeerConnection(RTCConfiguration configuration) : base()
    {
        NativeObject = new SIPSorcery.Net.RTCPeerConnection(configuration?.ToNative());

        NativeObject.onconnectionstatechange += NativeOnConnectionStateChange;
        NativeObject.onicecandidate += NativeOnIceCandidate;
        NativeObject.ondatachannel += NativeOnDataChannel;
        NativeObject.onicecandidateerror += NativeOnIceCandidateError;
        NativeObject.oniceconnectionstatechange += NativeOnIceConnectionStateChange;
        NativeObject.onicegatheringstatechange += NativeOnIceGatheringStateChange;
        NativeObject.onnegotiationneeded += NativeOnNegotiationNeeded;
        NativeObject.onsignalingstatechange += NativeOnSignalingStateChange;
    }


    public bool CanTrickleIceCandidates => throw new NotImplementedException();

    public RTCPeerConnectionState ConnectionState => NativeObject.connectionState.FromNative();

    public RTCSessionDescriptionInit CurrentLocalDescription => throw new NotImplementedException();

    public RTCSessionDescriptionInit CurrentRemoteDescription => throw new NotImplementedException();

    public RTCIceConnectionState IceConnectionState => throw new NotImplementedException();

    public RTCIceGatheringState IceGatheringState => throw new NotImplementedException();

    public RTCSessionDescriptionInit LocalDescription => throw new NotImplementedException();

    public Task<IRTCIdentityAssertion> PeerIdentity => throw new NotImplementedException();

    public RTCSessionDescriptionInit PendingLocalDescription => throw new NotImplementedException();

    public RTCSessionDescriptionInit PendingRemoteDescription => throw new NotImplementedException();

    public RTCSessionDescriptionInit RemoteDescription => throw new NotImplementedException();

    public IRTCSctpTransport Sctp => throw new NotImplementedException();//// NativeObject.sctp.ToNative();

    public RTCSignalingState SignalingState => throw new NotImplementedException();

    public event EventHandler OnConnectionStateChanged;
    public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
    public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
    public event EventHandler OnIceConnectionStateChange;
    public event EventHandler OnIceGatheringStateChange;
    public event EventHandler OnNegotiationNeeded;
    public event EventHandler OnSignallingStateChange;
    public event EventHandler<IRTCTrackEvent> OnTrack;

    public Task AddIceCandidate(RTCIceCandidateInit candidate)
    {
        NativeObject.addIceCandidate(candidate.ToNative());
        return Task.CompletedTask;
    }

    public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream)
    {
        throw new NotImplementedException();
    }

    public IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind, RTCRtpTransceiverInit init = null)
    {
        throw new NotImplementedException();
    }

    public IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track, RTCRtpTransceiverInit init = null)
    {
        throw new NotImplementedException();
    }

    public void Close() => NativeObject.Close("");

    public Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options = null)
    {
        var nativeAnswerDescription = NativeObject.createAnswer(options?.ToNative());
        return Task.FromResult(nativeAnswerDescription.FromNative());
    }

    public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options)
    {
        SIPSorcery.Net.RTCDataChannel nativeDataChannel = null;
        Task.Run(() =>
        {
            nativeDataChannel = NativeObject.createDataChannel(label, options?.ToNative()).Result;
        }).Wait();//// RunSynchronously();

        return new RTCDataChannel(nativeDataChannel);
    }

    public Task<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions options = null)
    {
        var nativeOfferDescription = NativeObject.createOffer(options?.ToNative());
        return Task.FromResult(nativeOfferDescription.FromNative());
    }

    public void Dispose()
    {
    }

    public Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm)
    {
        throw new NotImplementedException();
    }

    public RTCConfiguration GetConfiguration()
    {
        throw new NotImplementedException();
    }

    public RTCIceServer[] GetDefaultIceServers()
    {
        throw new NotImplementedException();
    }

    public void GetIdentityAssertion()
    {
        throw new NotImplementedException();
    }

    public IRTCRtpReceiver[] GetReceivers()
    {
        throw new NotImplementedException();
    }

    public IRTCRtpSender[] GetSenders()
    {
        throw new NotImplementedException();
    }

    public Task<IRTCStatsReport> GetStats()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetStatsHack()
    {
        throw new NotImplementedException();
    }

    public IRTCRtpTransceiver[] GetTransceivers()
    {
        throw new NotImplementedException();
    }

    public void RemoveTrack(IRTCRtpSender sender)
    {
        throw new NotImplementedException();
    }

    public void RestartIce()
    {
        throw new NotImplementedException();
    }

    public void SetConfiguration(RTCConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public void SetIdentityProvider(string domainName, string protocol = null, string userName = null)
    {
        throw new NotImplementedException();
    }

    public Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription)
    {
        return NativeObject.setLocalDescription(sessionDescription.ToNative());
    }

    public Task SetRemoteDescription(RTCSessionDescriptionInit sessionDescription)
    {
        return Task.FromResult(NativeObject.setRemoteDescription(sessionDescription.ToNative()));
    }

    /*
     * 
     * Native events.
     * 
     */
    void NativeOnConnectionStateChange(SIPSorcery.Net.RTCPeerConnectionState connectionState)
    {
        //System.Diagnostics.Debug.WriteLine($"############################################ NativeOnConnectionStateChange: {connectionState}");
        OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
    }

    void NativeOnIceCandidate(SIPSorcery.Net.RTCIceCandidate iceCandidate)
    {
        //System.Diagnostics.Debug.WriteLine($"############################################ NativeOnIceCandidate: {iceCandidate}");
        OnIceCandidate?.Invoke(this, new RTCPeerConnectionIceEvent(iceCandidate));
    }

    private void NativeOnDataChannel(SIPSorcery.Net.RTCDataChannel obj)
    {
        //System.Diagnostics.Debug.WriteLine($"############################################ NativeOnDataChannel: {obj}");
        OnDataChannel?.Invoke(this, new RTCDataChannelEvent(obj));
    }

    private void NativeOnSignalingStateChange()
    {
        //System.Diagnostics.Debug.WriteLine($"############################################ NativeOnSignalingStateChange");
    }

    private void NativeOnNegotiationNeeded()
    {
        //System.Diagnostics.Debug.WriteLine($"############################################ NativeOnNegotiationNeeded");
    }

    private void NativeOnIceGatheringStateChange(SIPSorcery.Net.RTCIceGatheringState obj)
    {
        //System.Diagnostics.Debug.WriteLine($"############################################ NativeOnIceGatheringStateChange: {obj}");
    }

    private void NativeOnIceConnectionStateChange(SIPSorcery.Net.RTCIceConnectionState obj)
    {
        //System.Diagnostics.Debug.WriteLine($"############################################ NativeOnIceConnectionStateChange: {obj}");
    }


    private void NativeOnIceCandidateError(SIPSorcery.Net.RTCIceCandidate arg1, string arg2)
    {
        //System.Diagnostics.Debug.WriteLine($"############################################ NativeOnIceCandidateError: {arg1}, {arg2}");
    }


}

