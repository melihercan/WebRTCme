using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum RTCErrorDetailType
    {
        [EnumMember(Value = "data-channel-failure")]
        DataChannelFailure,

        [EnumMember(Value = "dtls-failure")]
        DtlsFailure,

        [EnumMember(Value = "fingerprint-failure")]
        FingerprintFailure,

        [EnumMember(Value = "sctp-failure")]
        SctpFailure,

        [EnumMember(Value = "sdp-syntax-error")]
        SdpSyntaxError,

        [EnumMember(Value = "hardware-encoder-not-available")]
        HardwareEncoderNotAvailable,

        [EnumMember(Value = "hardware-encoder-error")]
        HardwareEncoderError
    }
}
