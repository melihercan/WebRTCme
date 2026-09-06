using System.Runtime.InteropServices;
using static WebRTCme.Bindings.Maui.Windows.Interop;

namespace WebRTCme.Windows;

/// <summary>
/// Lays out an <see cref="RTCIceServer"/> array the way the ABI wants it: an array of structs
/// pointing at UTF-8 strings.
/// </summary>
/// <remarks>
/// The shim copies the configuration into WebRTC's own during
/// <c>rtc_peer_connection_create</c> and keeps none of these pointers, so all of it can be freed
/// as soon as that call returns -- which is what makes a <c>using</c> around the call correct.
/// </remarks>
internal sealed unsafe class NativeIceServers : IDisposable
{
    private readonly List<IntPtr> _strings = [];
    private IceServer* _servers;

    internal NativeIceServers(RTCIceServer[] iceServers)
    {
        var servers = iceServers ?? [];

        if (servers.Length > 0)
        {
            _servers = (IceServer*)NativeMemory.Alloc((nuint)servers.Length, (nuint)sizeof(IceServer));

            for (var index = 0; index < servers.Length; index++)
            {
                var server = servers[index];

                // The ABI takes the url list comma separated, matching the W3C shape.
                _servers[index] = new IceServer
                {
                    Urls = Utf8(string.Join(",", server.Urls ?? [])),
                    Username = Utf8(server.Username),
                    Password = Utf8(server.Credential)
                };
            }
        }

        Configuration = new Configuration
        {
            IceServers = (IntPtr)_servers,
            IceServerCount = servers.Length
        };
    }

    internal Configuration Configuration { get; }

    private IntPtr Utf8(string value)
    {
        if (value is null)
            return IntPtr.Zero;

        var pointer = Marshal.StringToCoTaskMemUTF8(value);
        _strings.Add(pointer);
        return pointer;
    }

    public void Dispose()
    {
        foreach (var pointer in _strings)
            Marshal.FreeCoTaskMem(pointer);
        _strings.Clear();

        if (_servers is not null)
        {
            NativeMemory.Free(_servers);
            _servers = null;
        }
    }
}
