using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// Browser-shaped device access over the interop shim's enumeration and track creation.
/// </summary>
/// <remarks>
/// The ABI deliberately does no constraint negotiation -- it takes concrete numbers -- so the
/// translation from a <see cref="MediaStreamConstraints"/> to a device id and a size lives here.
/// </remarks>
internal sealed class MediaDevices : IMediaDevices
{
    // What a camera is opened at when the caller asks for video without saying how.
    private const int DefaultWidth = 640;
    private const int DefaultHeight = 480;
    private const int DefaultFrameRate = 30;

    /// <summary>
    /// Never raised: the shim reports no device-change notifications. Declared because
    /// <see cref="IMediaDevices"/> requires it.
    /// </summary>
#pragma warning disable CS0067 // no device-change signal in the ABI
    public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;
#pragma warning restore CS0067

    public Task<MediaDeviceInfo[]> EnumerateDevices()
    {
        var factory = WebRtcRuntime.Factory;
        var devices = new List<MediaDeviceInfo>();

        WebRtcRuntime.Check(VideoDeviceCount(factory, out var cameraCount), "count video devices");
        for (var index = 0; index < cameraCount; index++)
        {
            if (VideoDeviceInfo(factory, index, out var name, out var id) != Ok)
                continue;

            devices.Add(new MediaDeviceInfo
            {
                DeviceId = WebRtcRuntime.TakeString(id),
                Label = WebRtcRuntime.TakeString(name),
                Kind = MediaDeviceInfoKind.VideoInput
            });
        }

        AddAudioDevices(factory, AudioDeviceRecording, MediaDeviceInfoKind.AudioInput, devices);
        AddAudioDevices(factory, AudioDevicePlayout, MediaDeviceInfoKind.AudioOutput, devices);

        foreach (var device in devices)
            device.GroupId = device.DeviceId;

        return Task.FromResult(devices.ToArray());
    }

    private static void AddAudioDevices(IntPtr factory, int abiKind, MediaDeviceInfoKind kind,
                                        List<MediaDeviceInfo> devices)
    {
        WebRtcRuntime.Check(AudioDeviceCount(factory, abiKind, out var count),
                            $"count {kind} devices");

        for (var index = 0; index < count; index++)
        {
            if (AudioDeviceInfo(factory, abiKind, index, out var name, out var id) != Ok)
                continue;

            devices.Add(new MediaDeviceInfo
            {
                DeviceId = WebRtcRuntime.TakeString(id),
                Label = WebRtcRuntime.TakeString(name),
                Kind = kind
            });
        }
    }

    public MediaTrackSupportedConstraints GetSupportedConstraints() => new()
    {
        DeviceId = true,
        GroupId = true,
        Width = true,
        Height = true,
        FrameRate = true
    };

    public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints) =>
        throw new NotSupportedException(
            "Screen capture is not exposed by the Windows binding; the interop ABI has no " +
            "desktop capture source.");

    public Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints)
    {
        var wantsAudio = IsRequested(constraints?.Audio);
        var wantsVideo = IsRequested(constraints?.Video);

        if (!wantsAudio && !wantsVideo)
            throw new ArgumentException(
                "At least one of audio or video must be requested.", nameof(constraints));

        var factory = WebRtcRuntime.Factory;
        var tracks = new List<IMediaStreamTrack>();

        try
        {
            if (wantsAudio)
                tracks.Add(CreateAudioTrack(factory));

            if (wantsVideo)
                tracks.Add(CreateVideoTrack(factory, constraints?.Video?.Object));
        }
        catch
        {
            // Do not leave a half-open microphone behind if the camera fails.
            foreach (var track in tracks)
                track.Dispose();
            throw;
        }

        return Task.FromResult<IMediaStream>(new MediaStream(tracks));
    }

    private static bool IsRequested(MediaStreamContraintsUnion union) =>
        union is not null && (union.Value == true || union.Object is not null);

    /// <summary>
    /// Opens the default recording device. The ABI takes no device id for audio -- the audio
    /// device module follows the system default -- so an audio deviceId constraint cannot be
    /// honoured, and asking for a specific microphone is not yet possible.
    /// </summary>
    private static IMediaStreamTrack CreateAudioTrack(IntPtr factory)
    {
        var id = NewTrackId("audio");
        WebRtcRuntime.Check(AudioTrackCreate(factory, id, out var handle), "create an audio track");

        return new MediaStreamTrack(handle, MediaStreamTrackKind.Audio, id,
                                    label: "Default audio input", isRemote: false);
    }

    private IMediaStreamTrack CreateVideoTrack(IntPtr factory, MediaTrackConstraints constraints)
    {
        var requestedDeviceId = constraints?.DeviceId?.Value
            ?? constraints?.DeviceId?.Exact?.Value
            ?? constraints?.DeviceId?.Ideal?.Value;

        var (deviceId, label) = ResolveCamera(factory, requestedDeviceId);

        var width = (int?)constraints?.Width?.Value ?? DefaultWidth;
        var height = (int?)constraints?.Height?.Value ?? DefaultHeight;
        var frameRate = (int?)constraints?.FrameRate?.Value ?? DefaultFrameRate;

        // The id travels into the SDP as the msid, so it must not be the device path.
        var id = NewTrackId("video");

        WebRtcRuntime.Check(
            VideoTrackCreate(factory, deviceId, id, width, height, frameRate, out var handle),
            $"open camera '{label}' at {width}x{height}@{frameRate}");

        return new MediaStreamTrack(handle, MediaStreamTrackKind.Video, id, label,
                                    isRemote: false, deviceId, width, height, frameRate);
    }

    /// <summary>
    /// Picks the camera to open, so the track reports the device actually in use rather than
    /// echoing back an unspecified request.
    /// </summary>
    private (string DeviceId, string Label) ResolveCamera(IntPtr factory, string requestedDeviceId)
    {
        var cameras = EnumerateDevices().GetAwaiter().GetResult()
            .Where(device => device.Kind == MediaDeviceInfoKind.VideoInput)
            .ToArray();

        if (cameras.Length == 0)
            throw new InvalidOperationException("No video capture device is available.");

        if (requestedDeviceId is null)
            return (cameras[0].DeviceId, cameras[0].Label);

        var requested = cameras.FirstOrDefault(device => device.DeviceId == requestedDeviceId)
            ?? throw new InvalidOperationException(
                $"No video capture device with id '{requestedDeviceId}'.");

        return (requested.DeviceId, requested.Label);
    }

    private static string NewTrackId(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    public void Dispose() { }
}
