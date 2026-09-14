using Microsoft.Extensions.DependencyInjection;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRTCme.PackageTests;

/// <summary>
/// Touches the surface a consumer of the packages actually touches, so that surface has to resolve
/// and compile on every target framework the package claims to support.
/// </summary>
/// <remarks>
/// <para>
/// Nothing here runs. It exists to be compiled: every type named below has to be reachable from the
/// package's <c>lib/</c> folder for this target framework, with its dependencies resolving. That is
/// a low bar and it is exactly the bar that packaging faults fail - <c>dotnet pack</c> drops build
/// output it does not recognise without a word, and a slice that went missing restores fine and
/// then has nothing in it.
/// </para>
/// <para>
/// Whether the native half then loads is a different question, and not one a compiler can answer.
/// That is tier 3.
/// </para>
/// </remarks>
public static class Consumer
{
    /// <summary>The plug-in entry point: how a consumer reaches the platform's WebRTC.</summary>
    public static IWebRtc PlugIn() => CrossWebRtc.Current;

    /// <summary>The configuration and enum surface, which is the W3C shape the API mirrors.</summary>
    public static RTCConfiguration Configuration() => new()
    {
        IceServers =
        [
            new RTCIceServer { Urls = ["stun:stun.l.google.com:19302"] }
        ],
        IceTransportPolicy = RTCIceTransportPolicy.All,
        BundlePolicy = RTCBundlePolicy.MaxBundle
    };

    /// <summary>
    /// The middleware's registration entry point. This is the one that reaches furthest: it pulls
    /// in the connection layer and the mediasoup client, all of which are folded into the package
    /// rather than referenced, so a missing fold shows up right here.
    /// </summary>
    public static IServiceCollection Services() => new ServiceCollection().AddMiddleware();

    /// <summary>The view models a consumer binds to, and the media types they hand back.</summary>
    public static Type[] MiddlewareSurface() =>
    [
        typeof(CallViewModel),
        typeof(ChatViewModel),
        typeof(ConnectionParametersViewModel),
        typeof(IMediaStreamManager),
        typeof(ILocalMediaStream)
    ];

    /// <summary>The core API types, all of which live in the WebRTCme package.</summary>
    public static Type[] ApiSurface() =>
    [
        typeof(IMediaDevices),
        typeof(IMediaStream),
        typeof(IMediaStreamTrack),
        typeof(IRTCPeerConnection),
        typeof(IRTCDataChannel),
        typeof(MediaStreamConstraints),
        typeof(RTCIceCandidateInit),
        typeof(RTCSessionDescriptionInit)
    ];
}
