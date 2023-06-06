using System.Runtime.InteropServices;

namespace WebRTCme.Bindings.Native;

public static class Registry
{
    public const string NativeLibWindows = "webrtc.dll";
    public const string NativeLibMacOs = "libwebrtc.dylib";
    public const string NativeLibLinux = "libwebrtc.so";

    public const string InteropLibWindows = "WebRtcInterop.dll";
    public const string InteropLibMacOs = "libWebRtcInterop.dylib";
    public const string InteropLibLinux = "libWebRtcInterop.so";


#if WINDOWS
    public const string NativeLib = NativeLibWindows;
    public const string InteropLib = InteropLibWindows;
#elif MACOS
    public const string NativeLib = NativeLibMacOs;
    public const string InteropLib = InteropLibMacOs;
#elif LINUX
    public const string NativeLib = NativeLibLinux;
    public const string InteropLib = InteropLibLinux;
#else
    public const string NativeLib = " ";
    public const string InteropLib = " ";
#endif


}