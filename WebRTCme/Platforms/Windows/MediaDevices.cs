using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SIPSorcery.Media;
using SIPSorceryMedia.FFmpeg;
using SIPSorceryMedia.Windows;
using WebRTCme.Shared.SipSorcery.Media;

namespace WebRTCme.Windows
{
    /// <summary>
    /// Browser-shaped media device access backed by the SIPSorceryMedia.Windows capture
    /// endpoints (NAudio for microphones, Media Foundation for cameras).
    /// </summary>
    internal class MediaDevices : IMediaDevices
    {
        // Defaults used when the caller supplies no video size/rate constraints. Zero lets
        // WindowsVideoEndPoint pick the device's own preferred format.
        private const uint DefaultWidth = 0;
        private const uint DefaultHeight = 0;
        private const uint DefaultFps = 0;

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public async Task<MediaDeviceInfo[]> EnumerateDevices()
        {
            var videoDevices = await WindowsVideoEndPoint.GetVideoCatpureDevices();

            return videoDevices
                .Select(device => new MediaDeviceInfo
                {
                    DeviceId = device.ID,
                    GroupId = device.ID,
                    Kind = MediaDeviceInfoKind.VideoInput,
                    Label = device.Name
                })
                .ToArray();
        }

        public MediaTrackSupportedConstraints GetSupportedConstraints() => new()
        {
            DeviceId = true,
            Width = true,
            Height = true,
            FrameRate = true
        };

        public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints) =>
            throw new NotSupportedException(
                "Screen capture is not supported by the SipSorcery binding.");

        public async Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints)
        {
            var wantsAudio = IsRequested(constraints?.Audio);
            var wantsVideo = IsRequested(constraints?.Video);

            if (!wantsAudio && !wantsVideo)
                throw new ArgumentException(
                    "At least one of audio or video must be requested.", nameof(constraints));

            var tracks = new List<IMediaStreamTrack>();

            if (wantsAudio)
                tracks.Add(await CreateAudioTrackAsync());

            if (wantsVideo)
                tracks.Add(await CreateVideoTrackAsync(constraints?.Video?.Object));

            return new WebRTCme.Shared.SipSorcery.Media.MediaStream(tracks);
        }

        public void Dispose() { }

        private static bool IsRequested(MediaStreamContraintsUnion union) =>
            union is not null && (union.Value == true || union.Object is not null);

        private static async Task<IMediaStreamTrack> CreateAudioTrackAsync()
        {
            var audioEndPoint = new WindowsAudioEndPoint(new AudioEncoder());
            await audioEndPoint.StartAudio();

            return new WebRTCme.Shared.SipSorcery.Media.MediaStreamTrack(audioEndPoint,
                deviceId: string.Empty, label: "Default audio input");
        }

        private static async Task<IMediaStreamTrack> CreateVideoTrackAsync(
            MediaTrackConstraints videoConstraints)
        {
            var deviceId = videoConstraints?.DeviceId?.Value
                ?? videoConstraints?.DeviceId?.Exact?.Value
                ?? videoConstraints?.DeviceId?.Ideal?.Value;

            var width = (uint?)videoConstraints?.Width?.Value ?? DefaultWidth;
            var height = (uint?)videoConstraints?.Height?.Value ?? DefaultHeight;
            var fps = (uint?)videoConstraints?.FrameRate?.Value ?? DefaultFps;

            var (resolvedDeviceId, label) = await ResolveVideoDeviceAsync(deviceId);

            FFmpegRuntime.EnsureInitialised();

            var videoEndPoint = new WindowsVideoEndPoint(new FFmpegVideoEncoder(), deviceId,
                width, height, fps);

            if (!await videoEndPoint.InitialiseVideoSourceDevice())
                throw new InvalidOperationException(
                    deviceId is null
                        ? "Failed to initialise the default video capture device."
                        : $"Failed to initialise video capture device '{deviceId}'.");

            // The endpoint emits no frames at all until a source format is selected, so pick a
            // default up front. Without this the local preview stays black until a peer
            // connection negotiates a format - and never lights up if there is no call.
            var sourceFormats = videoEndPoint.GetVideoSourceFormats();
            if (sourceFormats.Count > 0)
                videoEndPoint.SetVideoSourceFormat(sourceFormats[0]);

            await videoEndPoint.StartVideo();

            return new WebRTCme.Shared.SipSorcery.Media.MediaStreamTrack(videoEndPoint,
                resolvedDeviceId, label, width, height, fps);
        }

        /// <summary>
        /// Resolves the device that will actually be captured from, so that the track reports the
        /// real device rather than echoing back a null/unspecified request.
        /// </summary>
        private static async Task<(string DeviceId, string Label)> ResolveVideoDeviceAsync(
            string requestedDeviceId)
        {
            // VideoCaptureDeviceInfo is a struct, so FirstOrDefault yields a default instance
            // with null fields rather than null.
            var devices = await WindowsVideoEndPoint.GetVideoCatpureDevices();

            if (requestedDeviceId is not null)
            {
                var requested = devices.FirstOrDefault(device => device.ID == requestedDeviceId);
                return (requestedDeviceId, requested.Name ?? requestedDeviceId);
            }

            // A null device id makes WindowsVideoEndPoint fall back to the first camera.
            var first = devices.FirstOrDefault();
            return (first.ID ?? string.Empty, first.Name ?? "Default video input");
        }
    }
}
