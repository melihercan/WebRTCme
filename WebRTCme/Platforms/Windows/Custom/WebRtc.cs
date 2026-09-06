using Microsoft.JSInterop;

namespace WebRTCme;

internal sealed class WebRtc : IWebRtc
{
    public static IWebRtc Create() => new WebRtc();

    /// <summary>
    /// The JS runtime is meaningless here -- it exists for the Blazor binding, which is the only
    /// one that reaches WebRTC through the browser -- and is ignored.
    /// </summary>
    public IWindow Window(IJSRuntime jsRuntime = null) => new WebRTCme.Windows.Window();

    public void Dispose() { }
}
