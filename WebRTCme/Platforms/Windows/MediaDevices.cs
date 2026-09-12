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
    /// <remarks>
    /// The absence is real and it costs something - see <see cref="CaptureDeviceWatcher"/>, which
    /// has to poll <see cref="EnumerateInputDevices"/> to notice a camera being unplugged. Raising
    /// this properly would need a callback added to the ABI, whose source is not in this
    /// repository.
    /// </remarks>
#pragma warning disable CS0067 // no device-change signal in the ABI
    public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;
#pragma warning restore CS0067

    /// <summary>
    /// The ids of the capture devices currently present, cameras and microphones.
    /// </summary>
    /// <remarks>
    /// Split out of <see cref="EnumerateDevices"/> so that watching for a device to disappear
    /// costs only the enumeration and not the <see cref="MediaDeviceInfo"/> objects and marshalled
    /// labels around it - this runs every couple of seconds for the length of a call. Playout
    /// devices are left out: nothing is captured from them, so nothing here can lose one.
    /// </remarks>
    internal static IEnumerable<string> EnumerateInputDevices()
    {
        var factory = WebRtcRuntime.Factory;

        WebRtcRuntime.Check(VideoDeviceCount(factory, out var cameraCount), "count video devices");
        for (var index = 0; index < cameraCount; index++)
        {
            if (VideoDeviceInfo(factory, index, out var name, out var id) != Ok)
                continue;

            WebRtcRuntime.TakeString(name);
            yield return WebRtcRuntime.TakeString(id);
        }

        WebRtcRuntime.Check(AudioDeviceCount(factory, AudioDeviceRecording, out var micCount),
                            "count AudioInput devices");
        for (var index = 0; index < micCount; index++)
        {
            if (AudioDeviceInfo(factory, AudioDeviceRecording, index, out var name, out var id) != Ok)
                continue;

            WebRtcRuntime.TakeString(name);
            yield return WebRtcRuntime.TakeString(id);
        }
    }

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

    /// <summary>
    /// Captures the primary screen. The W3C call shows a picker and returns what the user
    /// chose; there is no picker here, so it takes the first screen. An app that wants a
    /// choice enumerates with <see cref="WindowsSupport.GetDesktopSources"/> and calls
    /// <see cref="WindowsSupport.GetDisplayMedia"/> with the one it wants.
    /// </summary>
    public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
    {
        var screens = WindowsSupport.GetDesktopSources(DesktopSourceKind.Screen);
        if (screens.Count == 0)
            throw new InvalidOperationException("No screen is available to capture.");

        var frameRate = (int?)constraints?.Video?.Object?.FrameRate?.Value ?? DefaultShareFrameRate;
        return Task.FromResult(WindowsSupport.GetDisplayMedia(screens[0], frameRate));
    }

    /// <summary>
    /// Shared screens are read, not watched: a lower rate leaves bandwidth for resolution, which
    /// is what keeps text legible.
    /// </summary>
    internal const int DefaultShareFrameRate = 15;

    internal static IMediaStreamTrack CreateDesktopTrack(int kind, long sourceId, string title,
                                                         int maxFrameRate)
    {
        var id = NewTrackId("screen");
        WebRtcRuntime.Check(
            DesktopTrackCreate(WebRtcRuntime.Factory, kind, sourceId, id, maxFrameRate,
                               out var handle),
            $"capture '{title}'");

        return new MediaStreamTrack(handle, MediaStreamTrackKind.Video, id, title,
                                    isRemote: false, deviceId: null,
                                    width: 0, height: 0, frameRate: maxFrameRate);
    }

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

        // Watched from here rather than from the constructor, because a remote track and a desktop
        // track go through the same type and neither is a device that can be unplugged. In
        // practice this watches the camera and not the microphone: the shim always opens the
        // default audio input and does not say which it was, so an audio track has no device id
        // to miss from a list.
        foreach (var track in tracks)
            CaptureDeviceWatcher.Watch(track as MediaStreamTrack);

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
        // Read through VideoConstraints rather than off the bare .Value fields: a caller writing
        // { width: { ideal: 1280 } } means it, and reading only the simplest of the forms answered
        // that with the default instead, silently.
        //
        // facingMode is not read here and cannot be. A desktop camera has no facing and the shim
        // reports none, so CameraType.Front and CameraType.Back both open the default camera -
        // which is what they did before, and is the only honest answer on this platform.
        var videoConstraints = VideoConstraints.From(constraints);

        var (deviceId, label) = ResolveCamera(factory, videoConstraints.DeviceId);

        var width = videoConstraints.Width ?? DefaultWidth;
        var height = videoConstraints.Height ?? DefaultHeight;
        var frameRate = videoConstraints.FrameRate ?? DefaultFrameRate;

        // The id travels into the SDP as the msid, so it must not be the device path.
        var id = NewTrackId("video");

        var status = VideoTrackCreate(factory, deviceId, id, width, height, frameRate,
                                      out var handle);

        // The shim separates a device that is absent from one that is present but will not
        // start. Only the second is actionable, and "not found" for a camera the user can see
        // listed sends them looking for the wrong problem.
        if (status == ErrInvalidState)
            throw new InvalidOperationException(
                $"The camera '{label}' was found but could not be started at " +
                $"{width}x{height}@{frameRate}. It is usually held by another application; " +
                "close anything else using the camera and try again.");

        WebRtcRuntime.Check(status, $"open camera '{label}' at {width}x{height}@{frameRate}");

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
