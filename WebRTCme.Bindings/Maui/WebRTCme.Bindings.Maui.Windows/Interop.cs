using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebRTCme.Bindings.Maui.Windows;

/// <summary>
/// The C ABI exposed by WebRtcInterop.dll, mirrored one declaration at a time.
/// </summary>
/// <remarks>
/// <para>
/// This file is deliberately mechanical: every entry corresponds to a line in
/// <c>WebRtcInterop/include/Interop.h</c> and nothing here interprets, wraps or
/// improves on it. Keeping it dumb is what makes it reviewable against the
/// header. Everything that needs judgement — lifetime, threading, turning
/// callbacks into observables — belongs a layer up.
/// </para>
/// <para>
/// The contract, including the rules the comments below refer to, is the
/// "Interop ABI" page of the WebRTCnative wiki.
/// </para>
/// <para>
/// Two rules govern everything above this layer. Handles that arrive through an
/// out-parameter or a callback are owned by the receiver and need exactly one
/// matching release; strings that arrive as callback arguments are the
/// opposite, borrowed for the duration of the call, while strings returned
/// through an out-parameter must be freed with <see cref="StringFree"/>.
/// </para>
/// <para>
/// And no callback may call back into this class synchronously. Callbacks run
/// on WebRTC's signalling thread, which is shared by every peer connection the
/// process owns, and the calls here block on it: feeding a candidate raised by
/// one connection straight into another deadlocks that thread against itself.
/// Copy what is needed, queue it, and let an ordinary thread make the call.
/// </para>
/// </remarks>
public static partial class Interop
{
    private const string Lib = "WebRtcInterop";

    // ---------------------------------------------------------------- status

    public const int Ok = 0;
    public const int ErrInvalidArg = -1;
    public const int ErrInvalidState = -2;
    public const int ErrNotFound = -3;
    public const int ErrUnsupported = -4;
    public const int ErrInternal = -5;

    // ------------------------------------------------------------ enum values

    public const int PeerConnectionStateNew = 0;
    public const int PeerConnectionStateConnecting = 1;
    public const int PeerConnectionStateConnected = 2;
    public const int PeerConnectionStateDisconnected = 3;
    public const int PeerConnectionStateFailed = 4;
    public const int PeerConnectionStateClosed = 5;

    public const int SignalingStateStable = 0;
    public const int SignalingStateHaveLocalOffer = 1;
    public const int SignalingStateHaveLocalPrAnswer = 2;
    public const int SignalingStateHaveRemoteOffer = 3;
    public const int SignalingStateHaveRemotePrAnswer = 4;
    public const int SignalingStateClosed = 5;

    public const int MediaKindAudio = 0;
    public const int MediaKindVideo = 1;

    public const int DataChannelStateConnecting = 0;
    public const int DataChannelStateOpen = 1;
    public const int DataChannelStateClosing = 2;
    public const int DataChannelStateClosed = 3;

    public const int AudioDeviceRecording = 0;
    public const int AudioDevicePlayout = 1;

    // -------------------------------------------------------------- structs
    //
    // Verified blittable on x64 against the same clang-cl that builds the DLL,
    // so none of these needs [MarshalAs] or an explicit Pack. Field order is
    // the contract; do not reorder.

    [StructLayout(LayoutKind.Sequential)]
    public struct IceServer
    {
        public IntPtr Urls;      // UTF-8, comma separated
        public IntPtr Username;  // UTF-8, nullable
        public IntPtr Password;  // UTF-8, nullable
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Configuration
    {
        public IntPtr IceServers;      // IceServer*
        public int IceServerCount;
    }

    /// <summary>
    /// W3C RTCDataChannelInit. The optional members are plain ints with -1 for
    /// unset rather than nullables, matching the ABI: it keeps the struct
    /// blittable, and -1 is not a value any of these fields can take.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DataChannelInit
    {
        public IntPtr Protocol;        // UTF-8, nullable
        public int Ordered;            // 0 or 1; 1 is the W3C default
        public int MaxPacketLifeTime;  // milliseconds, or -1
        public int MaxRetransmits;     // or -1
        public int Negotiated;         // 0 or 1; 0 is the W3C default
        public int Id;                 // only meaningful when negotiated, else -1
    }

    /// <summary>An I420 frame. The planes are valid only for the duration of
    /// the callback that delivered them.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VideoFrame
    {
        public IntPtr Y;
        public IntPtr U;
        public IntPtr V;
        public int StrideY;
        public int StrideU;
        public int StrideV;
        public int Width;
        public int Height;
        public long TimestampUs;
    }

    /// <summary>
    /// Function pointers, not delegates. A delegate would need pinning for the
    /// lifetime of the registration and is the classic source of "worked in
    /// debug, crashed in release"; these point at
    /// <c>[UnmanagedCallersOnly]</c> statics, and per-instance context travels
    /// through the <c>userData</c> argument as a <see cref="GCHandle"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PeerConnectionObserver
    {
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr, void> OnIceCandidate;
        public delegate* unmanaged[Cdecl]<IntPtr, int, void> OnConnectionState;
        public delegate* unmanaged[Cdecl]<IntPtr, int, void> OnSignalingState;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr, void> OnTrack;
        public delegate* unmanaged[Cdecl]<IntPtr, void> OnRenegotiationNeeded;
        // Appended in the ABI rather than inserted, so the fields above keep
        // their offsets. Measured at 40; the struct is 48 bytes.
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> OnDataChannel;
    }

    /// <summary>
    /// Registered separately from the channel, because a channel arriving
    /// through <c>OnDataChannel</c> does not exist until the callback runs.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DataChannelObserver
    {
        public delegate* unmanaged[Cdecl]<IntPtr, int, void> OnState;
        public delegate* unmanaged[Cdecl]<IntPtr, byte*, int, int, void> OnMessage;
        public delegate* unmanaged[Cdecl]<IntPtr, ulong, void> OnBufferedAmountChange;
    }

    // -------------------------------------------------------------- library

    [LibraryImport(Lib, EntryPoint = "rtc_initialize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int Initialize();

    [LibraryImport(Lib, EntryPoint = "rtc_terminate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int Terminate();

    /// <summary>Frees any string this library returned through an out-parameter.</summary>
    [LibraryImport(Lib, EntryPoint = "rtc_string_free")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void StringFree(IntPtr s);

    // -------------------------------------------------------------- factory

    [LibraryImport(Lib, EntryPoint = "rtc_factory_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int FactoryCreate(out IntPtr factory);

    [LibraryImport(Lib, EntryPoint = "rtc_factory_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FactoryRelease(IntPtr factory);

    // -------------------------------------------------------------- devices
    //
    // Both out-strings are caller-owned; free each with StringFree.

    [LibraryImport(Lib, EntryPoint = "rtc_video_device_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int VideoDeviceCount(IntPtr factory, out int count);

    [LibraryImport(Lib, EntryPoint = "rtc_video_device_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int VideoDeviceInfo(IntPtr factory, int index, out IntPtr name, out IntPtr id);

    [LibraryImport(Lib, EntryPoint = "rtc_audio_device_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int AudioDeviceCount(IntPtr factory, int kind, out int count);

    [LibraryImport(Lib, EntryPoint = "rtc_audio_device_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int AudioDeviceInfo(IntPtr factory, int kind, int index, out IntPtr name, out IntPtr id);

    // --------------------------------------------------------------- tracks

    [LibraryImport(Lib, EntryPoint = "rtc_audio_track_create", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int AudioTrackCreate(IntPtr factory, string label, out IntPtr track);

    /// <param name="label">Becomes the track id, so it must be SDP-safe. Never
    /// pass the device id: a Windows device path carries backslashes and braces
    /// and would end up in the msid attribute.</param>
    [LibraryImport(Lib, EntryPoint = "rtc_video_track_create", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int VideoTrackCreate(IntPtr factory, string deviceId, string label,
                                               int width, int height, int fps, out IntPtr track);

    [LibraryImport(Lib, EntryPoint = "rtc_media_track_set_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int MediaTrackSetEnabled(IntPtr track, int enabled);

    [LibraryImport(Lib, EntryPoint = "rtc_media_track_get_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int MediaTrackGetId(IntPtr track, out IntPtr id);

    [LibraryImport(Lib, EntryPoint = "rtc_media_track_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MediaTrackRelease(IntPtr track);

    // ------------------------------------------------------- peer connection

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int PeerConnectionCreate(IntPtr factory, in Configuration config,
                                                   in PeerConnectionObserver observer,
                                                   IntPtr userData, out IntPtr pc);

    /// <summary>W3C close(): an observable state transition. The handle stays
    /// valid so callbacks already in flight can land; release separately.</summary>
    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int PeerConnectionClose(IntPtr pc);

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void PeerConnectionRelease(IntPtr pc);

    // ----------------------------------------------------------- negotiation
    //
    // These are asynchronous. The return value reports only whether the request
    // was accepted; completion arrives on the callback, on WebRTC's signalling
    // thread.

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_create_offer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int PeerConnectionCreateOffer(
        IntPtr pc,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> onSuccess,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFailure,
        IntPtr userData);

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_create_answer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int PeerConnectionCreateAnswer(
        IntPtr pc,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> onSuccess,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFailure,
        IntPtr userData);

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_set_local_description",
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int PeerConnectionSetLocalDescription(
        IntPtr pc, string type, string sdp,
        delegate* unmanaged[Cdecl]<IntPtr, void> onSuccess,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFailure,
        IntPtr userData);

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_set_remote_description",
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int PeerConnectionSetRemoteDescription(
        IntPtr pc, string type, string sdp,
        delegate* unmanaged[Cdecl]<IntPtr, void> onSuccess,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFailure,
        IntPtr userData);

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_add_ice_candidate",
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int PeerConnectionAddIceCandidate(IntPtr pc, string mid, int mlineIndex, string sdp);

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_add_track",
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int PeerConnectionAddTrack(IntPtr pc, IntPtr track, string streamId);

    // -------------------------------------------------------- data channels
    //
    // Creating a channel before the offer puts an m=application section in the
    // SDP; creating one afterwards raises OnRenegotiationNeeded, as in the W3C
    // API.

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_create_data_channel",
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int PeerConnectionCreateDataChannel(
        IntPtr pc, string label, in DataChannelInit init, out IntPtr channel);

    /// <summary>Register before the channel opens, or the open transition is
    /// missed -- for an incoming channel that means inside the callback that
    /// delivered it.</summary>
    [LibraryImport(Lib, EntryPoint = "rtc_data_channel_set_observer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DataChannelSetObserver(
        IntPtr channel, in DataChannelObserver observer, IntPtr userData);

    /// <summary>Asynchronous: the status says the payload was accepted on an
    /// open channel, not that it was delivered.</summary>
    [LibraryImport(Lib, EntryPoint = "rtc_data_channel_send")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int DataChannelSend(
        IntPtr channel, byte* data, int size, int isBinary);

    [LibraryImport(Lib, EntryPoint = "rtc_data_channel_get_label")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DataChannelGetLabel(IntPtr channel, out IntPtr label);

    [LibraryImport(Lib, EntryPoint = "rtc_data_channel_get_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DataChannelGetId(IntPtr channel, out int id);

    [LibraryImport(Lib, EntryPoint = "rtc_data_channel_get_state")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DataChannelGetState(IntPtr channel, out int state);

    [LibraryImport(Lib, EntryPoint = "rtc_data_channel_get_buffered_amount")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DataChannelGetBufferedAmount(IntPtr channel, out ulong amount);

    [LibraryImport(Lib, EntryPoint = "rtc_data_channel_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DataChannelClose(IntPtr channel);

    [LibraryImport(Lib, EntryPoint = "rtc_data_channel_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void DataChannelRelease(IntPtr channel);

    // --------------------------------------------------------- video frames
    //
    // The sink runs at capture or decode rate on a WebRTC thread and the planes
    // die when the callback returns. Copy or convert and return; anything
    // slower than the frame interval drops frames.

    [LibraryImport(Lib, EntryPoint = "rtc_video_track_add_sink")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int VideoTrackAddSink(
        IntPtr track,
        delegate* unmanaged[Cdecl]<IntPtr, VideoFrame*, void> onFrame,
        IntPtr userData);

    [LibraryImport(Lib, EntryPoint = "rtc_video_track_remove_sink")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int VideoTrackRemoveSink(IntPtr track);
}
