using System.Runtime.InteropServices;
using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// What <c>add_track</c> hands back: the handle through which a track can be swapped or removed
/// after the fact.
/// </summary>
/// <remarks>
/// <see cref="Track"/> is what this sender is currently sending, updated by
/// <see cref="ReplaceTrack"/> — so <c>GetSenders().First(s =&gt; s.Track.Kind == ...)</c> keeps
/// finding the right sender after a swap, which is how the signalling layer uses it.
/// </remarks>
internal sealed class RTCRtpSender : IRTCRtpSender
{
    private readonly RTCPeerConnection _peerConnection;
    private IntPtr _handle;

    /// <summary>
    /// The peer connection is held because it, not the sender, owns stats collection: WebRTC's
    /// per-sender getStats is a method on the connection taking the sender as a selector.
    /// </summary>
    internal RTCRtpSender(RTCPeerConnection peerConnection, IntPtr handle, IMediaStreamTrack track)
    {
        _peerConnection = peerConnection;
        _handle = handle;
        Track = track;
    }

    internal IntPtr Handle => _handle;

    public IMediaStreamTrack Track { get; private set; }

    /// <summary>
    /// Swaps the track without renegotiating — the whole point of replaceTrack over
    /// remove-then-add. A null track stops the sender while leaving the transport in place, which
    /// is how muting is done at the sender rather than the source.
    /// </summary>
    public Task ReplaceTrack(IMediaStreamTrack newTrack = null)
    {
        ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

        var handle = IntPtr.Zero;
        if (newTrack is not null)
        {
            if (newTrack is not MediaStreamTrack windowsTrack)
                throw new ArgumentException(
                    $"Track must come from the Windows binding, got {newTrack.GetType().FullName}.",
                    nameof(newTrack));

            if (newTrack.Kind != Track?.Kind)
                throw new ArgumentException(
                    $"A {Track?.Kind} sender cannot take a {newTrack.Kind} track.",
                    nameof(newTrack));

            handle = windowsTrack.Handle;
        }

        WebRtcRuntime.Check(RtpSenderReplaceTrack(_handle, handle),
                            "replace the track on the sender");

        Track = newTrack;
        return Task.CompletedTask;
    }

    /// <summary>Called after the peer connection has removed this sender.</summary>
    internal void Detach() => Track = null;

    public IRTCDTMFSender Dtmf =>
        throw new NotSupportedException("DTMF is not supported by the Windows binding.");

    public IRTCDtlsTransport Transport =>
        throw new NotSupportedException("Transport details are not exposed by the Windows binding.");

    public RTCRtpCapabilities GetCapabilities(string kind) =>
        throw new NotSupportedException("Sender capabilities are not reported by the Windows binding.");

    /// <summary>
    /// The sender's current encodings. <c>TransactionId</c>, <c>Codecs</c>,
    /// <c>HeaderExtensions</c> and <c>Rtcp</c> are null: the transaction id deliberately stays
    /// inside the shim, and the rest are read-only detail the ABI does not carry. The normal use
    /// -- read, change an encoding, write back -- needs none of them.
    /// </summary>
    public RTCRtpSendParameters GetParameters()
    {
        ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);
        return new RTCRtpSendParameters { Encodings = ReadEncodings(out _) };
    }

    /// <summary>
    /// Reads the native encodings, handing back the scalability modes separately because
    /// <see cref="RTCRtpEncodingParameters"/> has no field for them.
    /// </summary>
    private unsafe RTCRtpEncodingParameters[] ReadEncodings(out string[] scalabilityModes)
    {
        WebRtcRuntime.Check(RtpSenderGetParameters(_handle, null, 0, out var count),
                            "count the sender's encodings");
        if (count == 0)
        {
            scalabilityModes = [];
            return [];
        }

        var native = new RtpEncoding[count];
        fixed (RtpEncoding* buffer = native)
        {
            WebRtcRuntime.Check(RtpSenderGetParameters(_handle, buffer, count, out _),
                                "read the sender's encodings");
        }

        var encodings = new RTCRtpEncodingParameters[count];
        scalabilityModes = new string[count];
        for (var i = 0; i < count; i++)
        {
            // TakeString frees what the shim allocated, so reading is also what cleans up.
            var rid = WebRtcRuntime.TakeString(native[i].Rid);
            scalabilityModes[i] = WebRtcRuntime.TakeString(native[i].ScalabilityMode);

            encodings[i] = new RTCRtpEncodingParameters
            {
                Rid = rid,
                Active = native[i].Active != 0,
                MaxBitrate = native[i].MaxBitrate < 0 ? null : (ulong)native[i].MaxBitrate,
                MaxFramerate = native[i].MaxFramerate < 0 ? null : native[i].MaxFramerate,
                ScaleResolutionDownBy = native[i].ScaleResolutionDownBy > 0
                    ? native[i].ScaleResolutionDownBy
                    : null
            };
        }

        return encodings;
    }

    /// <summary>
    /// getStats() for this sender alone, rather than the whole connection.
    /// </summary>
    public Task<IRTCStatsReport> GetStats()
    {
        ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);
        return _peerConnection.GetSenderStats(_handle);
    }

    /// <summary>
    /// Changes what the sender transmits without renegotiating — a bitrate ceiling, a smaller
    /// picture, fewer frames. A null on an optional field clears it, which is how a cap is lifted.
    /// </summary>
    /// <remarks>
    /// Only the encodings are read; everything else W3C carries here is read-only. The count may
    /// not change, because WebRTC does not allow it.
    /// </remarks>
    public unsafe Task SetParameters(RTCRtpSendParameters parameters)
    {
        ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);
        ArgumentNullException.ThrowIfNull(parameters);

        var encodings = parameters.Encodings
            ?? throw new ArgumentException("Parameters carry no encodings.", nameof(parameters));

        // Read first, and only to carry the scalability mode across: the common API has no field
        // for it, and the ABI treats a null as "clear", so without this every SetParameters would
        // silently drop a mode the sender was negotiated with.
        var current = ReadEncodings(out var scalabilityModes);
        if (encodings.Length != current.Length)
            throw new ArgumentException(
                $"This sender has {current.Length} encoding(s); setParameters cannot change the " +
                $"count, and {encodings.Length} were given.", nameof(parameters));

        var native = new RtpEncoding[encodings.Length];
        var modes = new IntPtr[encodings.Length];
        try
        {
            for (var i = 0; i < encodings.Length; i++)
            {
                var encoding = encodings[i]
                    ?? throw new ArgumentException($"Encoding {i} is null.", nameof(parameters));

                modes[i] = scalabilityModes[i] is null
                    ? IntPtr.Zero
                    : Marshal.StringToCoTaskMemUTF8(scalabilityModes[i]);

                native[i] = new RtpEncoding
                {
                    // Left null on purpose: the shim keeps the rid negotiation settled on, and
                    // WebRTC rejects a change to it.
                    Rid = IntPtr.Zero,
                    Active = encoding.Active ? 1 : 0,
                    MaxBitrate = ToBitrate(encoding.MaxBitrate),
                    MaxFramerate = encoding.MaxFramerate is null
                        ? -1
                        : (int)Math.Round(encoding.MaxFramerate.Value),
                    ScaleResolutionDownBy = encoding.ScaleResolutionDownBy ?? 0d,
                    ScalabilityMode = modes[i]
                };
            }

            fixed (RtpEncoding* buffer = native)
            {
                WebRtcRuntime.Check(RtpSenderSetParameters(_handle, buffer, native.Length),
                                    "apply the sender parameters");
            }
        }
        finally
        {
            foreach (var mode in modes)
            {
                if (mode != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(mode);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// The ABI carries a bitrate as a 32-bit int with -1 for "no cap". Clamped rather than
    /// checked: a request above 2 Gbit/s is not a number anyone meant, and throwing on it would
    /// be a worse answer than sending the highest cap there is.
    /// </summary>
    private static int ToBitrate(ulong? bitsPerSecond) => bitsPerSecond switch
    {
        null => -1,
        > int.MaxValue => int.MaxValue,
        _ => (int)bitsPerSecond.Value
    };

    public void SetStreams(IMediaStream[] mediaStreams) =>
        throw new NotSupportedException(
            "Reassigning a sender's streams is not supported by the Windows binding.");

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle != IntPtr.Zero)
            RtpSenderRelease(handle);
    }
}
