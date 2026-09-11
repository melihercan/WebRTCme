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

    // W3C RTCRtpTransceiverDirection. Stopped is reported but never accepted: a transceiver is
    // stopped by stopping it, not by being described as stopped.
    public const int TransceiverDirectionSendRecv = 0;
    public const int TransceiverDirectionSendOnly = 1;
    public const int TransceiverDirectionRecvOnly = 2;
    public const int TransceiverDirectionInactive = 3;
    public const int TransceiverDirectionStopped = 4;

    public const int DataChannelStateConnecting = 0;
    public const int DataChannelStateOpen = 1;
    public const int DataChannelStateClosing = 2;
    public const int DataChannelStateClosed = 3;

    public const int AudioDeviceRecording = 0;
    public const int AudioDevicePlayout = 1;

    public const int DesktopSourceScreen = 0;
    public const int DesktopSourceWindow = 1;

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

    // ----------------------------------------------------------- statistics
    //
    // Asynchronous like negotiation: the return value reports only that
    // collection started. The report arrives on the callback as JSON, borrowed
    // for the duration of the call like every other callback string.

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_get_stats")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int PeerConnectionGetStats(
        IntPtr pc,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onSuccess,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFailure,
        IntPtr userData);

    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_add_ice_candidate",
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int PeerConnectionAddIceCandidate(IntPtr pc, string mid, int mlineIndex, string sdp);

    /// <param name="sender">Receives a handle to release. Pass <see cref="IntPtr.Zero"/> by
    /// using the overload-free form only when the track will never be replaced or removed.</param>
    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_add_track",
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int PeerConnectionAddTrack(IntPtr pc, IntPtr track, string streamId,
                                                     out IntPtr sender);

    // -------------------------------------------------------- desktop capture
    //
    // getDisplayMedia taken apart the same way getUserMedia was. Enumeration
    // takes no factory because a capturer needs none, and the id rather than
    // the index identifies a source -- indices are not stable across calls.

    [LibraryImport(Lib, EntryPoint = "rtc_desktop_source_count")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DesktopSourceCount(int kind, out int count);

    [LibraryImport(Lib, EntryPoint = "rtc_desktop_source_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DesktopSourceInfo(int kind, int index, out IntPtr title, out long id);

    [LibraryImport(Lib, EntryPoint = "rtc_desktop_track_create",
                   StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int DesktopTrackCreate(IntPtr factory, int kind, long sourceId,
                                                  string label, int maxFps, out IntPtr track);

    // --------------------------------------------------------------- senders

    /// <summary>W3C replaceTrack. A zero track stops the sender without
    /// renegotiating; the new track must be the same kind as the old.</summary>
    [LibraryImport(Lib, EntryPoint = "rtc_rtp_sender_replace_track")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpSenderReplaceTrack(IntPtr sender, IntPtr track);

    /// <summary>The sender handle stays valid and must still be released.</summary>
    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_remove_track")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int PeerConnectionRemoveTrack(IntPtr pc, IntPtr sender);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_sender_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void RtpSenderRelease(IntPtr sender);

    /// <summary>
    /// Audio or video. Needed for a track reached through a receiver, which arrived through
    /// negotiation and so carries no kind the caller already knows.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "rtc_media_track_get_kind")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int MediaTrackGetKind(IntPtr track, out int kind);

    /// <summary>
    /// getStats() narrowed to one sender, which the connection-wide call cannot do.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "rtc_rtp_sender_get_stats")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int RtpSenderGetStats(
        IntPtr pc,
        IntPtr sender,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onSuccess,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFailure,
        IntPtr userData);

    // --------------------------------------------------------- transceivers
    //
    // The unified-plan view: one transceiver per m-section, pairing a sender
    // with a receiver. Anything that negotiates per m-section needs these --
    // reading a mid, asking for specific simulcast encodings, or matching an
    // incoming stream to the section carrying it. Without them addTrack is the
    // only way to send, which is why mediasoup could not run on Windows.

    /// <summary>
    /// One simulcast layer. Optional members use a sentinel rather than a nullable, to keep the
    /// struct blittable: -1 for the integers, 0 for the scale factor, null for the strings.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RtpEncoding
    {
        public IntPtr Rid;                    // UTF-8, or Zero
        public int Active;                    // 0 or 1
        public int MaxBitrate;                // bits per second, or -1
        public int MaxFramerate;              // or -1
        public double ScaleResolutionDownBy;  // 0 means unset
        public IntPtr ScalabilityMode;        // UTF-8, or Zero
    }

    /// <param name="kind">Ignored when <paramref name="track"/> is non-zero.</param>
    /// <param name="track">Zero adds a transceiver of <paramref name="kind"/> with no track,
    /// which is how the platform's encoding capabilities are discovered.</param>
    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_add_transceiver")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int PeerConnectionAddTransceiver(
        IntPtr pc,
        int kind,
        IntPtr track,
        int direction,
        IntPtr* streamIds,
        int streamIdCount,
        RtpEncoding* encodings,
        int encodingCount,
        out IntPtr transceiver);

    /// <summary>
    /// Two-call: a null buffer counts, then a buffer of at least that size fills. A buffer that is
    /// too small is rejected having written nothing, so a caller that raced a renegotiation can
    /// simply ask again.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "rtc_peer_connection_get_transceivers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int PeerConnectionGetTransceivers(
        IntPtr pc, IntPtr* buffer, int capacity, out int count);

    /// <summary>
    /// Null until the local description naming it has been applied, reported as
    /// <c>RTC_ERR_NOT_FOUND</c> rather than as an empty string. Free with <c>StringFree</c>.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "rtc_rtp_transceiver_get_mid")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpTransceiverGetMid(IntPtr transceiver, out IntPtr mid);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_transceiver_get_direction")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpTransceiverGetDirection(IntPtr transceiver, out int direction);

    /// <summary>What negotiation settled on; <c>RTC_ERR_NOT_FOUND</c> until an answer arrives.</summary>
    [LibraryImport(Lib, EntryPoint = "rtc_rtp_transceiver_get_current_direction")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpTransceiverGetCurrentDirection(IntPtr transceiver,
                                                                out int direction);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_transceiver_set_direction")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpTransceiverSetDirection(IntPtr transceiver, int direction);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_transceiver_get_sender")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpTransceiverGetSender(IntPtr transceiver, out IntPtr sender);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_transceiver_get_receiver")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpTransceiverGetReceiver(IntPtr transceiver, out IntPtr receiver);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_transceiver_stop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpTransceiverStop(IntPtr transceiver);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_transceiver_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void RtpTransceiverRelease(IntPtr transceiver);

    // ------------------------------------------------------------ receivers

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_receiver_get_track")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int RtpReceiverGetTrack(IntPtr receiver, out IntPtr track);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_receiver_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void RtpReceiverRelease(IntPtr receiver);

    [LibraryImport(Lib, EntryPoint = "rtc_rtp_receiver_get_stats")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int RtpReceiverGetStats(
        IntPtr pc,
        IntPtr receiver,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onSuccess,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> onFailure,
        IntPtr userData);

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
