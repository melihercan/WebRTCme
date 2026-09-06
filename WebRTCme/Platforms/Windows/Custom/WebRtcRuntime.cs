using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// Owns the two things the C ABI keeps per process: the library itself and the peer connection
/// factory that every track and connection is created from.
/// </summary>
/// <remarks>
/// <para>
/// The ABI has no notion of more than one factory, and creating a second would give tracks that
/// cannot be added to connections made by the first, so this is deliberately a singleton rather
/// than something the caller can own.
/// </para>
/// <para>
/// Nothing calls <c>rtc_terminate</c>. It tears down the worker threads the library runs on, and
/// any handle still alive at that point becomes a use-after-free; there is no point in the
/// lifetime of a MAUI app where every track and connection is provably gone. Letting the process
/// exit with the library still up is what the platform does anyway.
/// </para>
/// </remarks>
internal static class WebRtcRuntime
{
    private static readonly object Gate = new();
    private static IntPtr _factory;

    /// <summary>The process-wide factory, started on first use.</summary>
    internal static IntPtr Factory
    {
        get
        {
            if (_factory != IntPtr.Zero)
                return _factory;

            lock (Gate)
            {
                if (_factory == IntPtr.Zero)
                {
                    Check(Initialize(), "initialise the WebRTC library");
                    Check(FactoryCreate(out var factory), "create the peer connection factory");
                    _factory = factory;
                }
            }

            return _factory;
        }
    }

    /// <summary>Throws unless the ABI reported success.</summary>
    internal static void Check(int status, string what)
    {
        if (status != Ok)
            throw new InvalidOperationException($"Failed to {what}: {Describe(status)}.");
    }

    /// <summary>Reads a UTF-8 string the ABI returned through an out-parameter, and frees it.</summary>
    internal static string TakeString(IntPtr utf8)
    {
        if (utf8 == IntPtr.Zero)
            return null;

        var value = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(utf8);
        StringFree(utf8);
        return value;
    }

    private static string Describe(int status) => status switch
    {
        ErrInvalidArg => "invalid argument",
        ErrInvalidState => "invalid state",
        ErrNotFound => "not found",
        ErrUnsupported => "not supported by this build",
        ErrInternal => "internal error",
        _ => $"status {status}"
    };
}
