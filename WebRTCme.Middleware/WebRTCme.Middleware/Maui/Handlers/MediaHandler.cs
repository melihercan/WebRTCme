#if IOS || MACCATALYST
using PlatformView = WebRTCme.Middleware.MediaHandler;
#elif ANDROID
using PlatformView = WebRTCme.Middleware.MediaHandler;
#elif WINDOWS
using PlatformView = WebRTCme.Middleware.MediaHandler;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace WebRTCme.Middleware;

public partial class MediaHandler
{
    public static IPropertyMapper<Media, MediaHandler> PropertyMapper = 
        new PropertyMapper<Media, MediaHandler>(ViewMapper)
    {
        [nameof(Media.Stream)] = MapStream,
        [nameof(Media.Hangup)] = MapHangup,
        [nameof(Media.Label)] = MapLabel,
        [nameof(Media.VideoMuted)] = MapVideoMuted,
        [nameof(Media.AudioMuted)] = MapAudioMuted,
        [nameof(Media.CameraType)] = MapCameraType,
        [nameof(Media.ShowControls)] = MapShowControls,
    };

    public static CommandMapper<Media, MediaHandler> CommandMapper = new(ViewCommandMapper)
    {
        // [nameof(Media.UpdateStatus)] = MapUpdateStatus,
    };

    public MediaHandler() : base(PropertyMapper, CommandMapper)
    {
    }
}