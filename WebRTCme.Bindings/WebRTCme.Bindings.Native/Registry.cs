using System.Runtime.InteropServices;

namespace WebRTCme.Bindings.Native;

public static class Registry
{
    public const string NativeLibWindows = "webrtc.dll";
    public const string NativeLibMacOs = "libwebrtc.dylib";
    public const string NativeLibLinux = "libwebrtc.so";


#if WINDOWS
    public const string NativeLib = NativeLibWindows;
#elif MACOS
    public const string NativeLib = NativeLibMacOs;
#elif LINUX
    public const string NativeLib = NativeLibLinux;
#else
    public const string NativeLib = " ";
#endif


}