using System.Text.Json;

namespace WebRTCme.Windows;

/// <summary>
/// An ICE candidate, with its fields read out of the SDP attribute the shim delivers.
/// </summary>
/// <remarks>
/// The ABI passes candidates as the raw attribute rather than a structured object, which is also
/// the form they travel in over signalling, so this parses on demand. The grammar is
/// RFC 5245 section 15.1:
/// <c>foundation component transport priority address port typ type [raddr .. rport ..] [tcptype ..]</c>.
/// </remarks>
internal sealed class RTCIceCandidate : IRTCIceCandidate
{
    private const string Prefix = "candidate:";

    private readonly string[] _fields;

    internal RTCIceCandidate(RTCIceCandidateInit init)
    {
        Candidate = init.Candidate;
        SdpMid = init.SdpMid;
        SdpMLineIndex = init.SdpMLineIndex;
        UsernameFragment = init.UsernameFragment;

        var value = Candidate ?? string.Empty;
        if (value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            value = value[Prefix.Length..];

        _fields = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    public string Candidate { get; }

    public string SdpMid { get; }

    public ushort? SdpMLineIndex { get; }

    public string UsernameFragment { get; }

    // The shim delivers the bare SDP attribute, with no record of the server it came from.
    public RTCIceServerTransportProtocol? RelayProtocol => null;

    public string Url => null;

    public string Foundation => Field(0);

    public RTCIceComponent Component =>
        Field(1) == "1" ? RTCIceComponent.Rtp : RTCIceComponent.Rtcp;

    public RTCIceProtocol Protocol =>
        Enum.TryParse<RTCIceProtocol>(Field(2), ignoreCase: true, out var protocol)
            ? protocol
            : RTCIceProtocol.Udp;

    public uint Priority => uint.TryParse(Field(3), out var priority) ? priority : 0;

    public string Address => Field(4);

    public string Ip => Address;

    public ushort Port => ushort.TryParse(Field(5), out var port) ? port : (ushort)0;

    public RTCIceCandidateType Type =>
        Enum.TryParse<RTCIceCandidateType>(Field(7), ignoreCase: true, out var type)
            ? type
            : RTCIceCandidateType.Host;

    public string RelatedAddress => Named("raddr");

    public ushort? RelatedPort =>
        ushort.TryParse(Named("rport"), out var port) ? port : null;

    public RTCIceTcpCandidateType? TcpType =>
        Enum.TryParse<RTCIceTcpCandidateType>(Named("tcptype"), ignoreCase: true, out var type)
            ? type
            : null;

    public string ToJson() => JsonSerializer.Serialize(this);

    private string Field(int index) => index < _fields.Length ? _fields[index] : null;

    /// <summary>Reads the value following a named marker, e.g. the address after "raddr".</summary>
    private string Named(string name)
    {
        for (var index = 0; index < _fields.Length - 1; index++)
        {
            if (string.Equals(_fields[index], name, StringComparison.OrdinalIgnoreCase))
                return _fields[index + 1];
        }

        return null;
    }

    public void Dispose() { }
}
