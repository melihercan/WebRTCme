using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class JsonIRTCIceCandidateArrayConverter : JsonConverter<IRTCIceCandidate[]>
    {
        public override IRTCIceCandidate[] Read(ref Utf8JsonReader reader, Type typeToConvert, 
            JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<IceCandidate[]>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, IRTCIceCandidate[] value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    internal class IceCandidate : IRTCIceCandidate
    {
        public string Candidate { get; set; }

        public RTCIceComponent Component { get; set; }

        public string Foundation { get; set; }

        public string Ip { get; set; }

        public ushort Port { get; set; }

        public uint Priority { get; set; }

        public string Address { get; set; }

        public RTCIceProtocol Protocol { get; set; }

        public string RelatedAddress { get; set; }

        public ushort? RelatedPort { get; set; }

        public string SdpMid { get; set; }

        public ushort? SdpMLineIndex { get; set; }

        public RTCIceTcpCandidateType? TcpType { get; set; }

        public RTCIceCandidateType Type { get; set; }

        public string UsernameFragment { get; set; }

        public object NativeObject { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetNativeObject()
        {
            throw new NotImplementedException();
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
