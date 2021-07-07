using System;
using System.Collections.Generic;

namespace SDPLib.Serializers
{
    class OptionalValueDeSerializer
    {
        public static readonly OptionalValueDeSerializer Instance = new OptionalValueDeSerializer();

        public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            if (data.Length == 0)
            {
                throw new DeserializationException("Invalid SDP field");
            }

            switch (data[0])
            {
                case SessionInformationSerializer.Identifier:
                    return SessionInformationSerializer.Instance.ReadValue(data, session);
                case ConnectionDataSerializer.Identifier:
                    session.ParsedValue.ConnectionData = ConnectionDataSerializer.Instance.ReadValue(data);
                    return this.ReadValue;
                case TimingSerializer.Identifier:
                    session.ParsedValue.Timings = session.ParsedValue.Timings ?? new List<TimingInfo>();
                    session.ParsedValue.Timings.Add(TimingSerializer.Instance.ReadValue(data));
                    return OptionalValueDeSerializer.Instance.ReadValue;
                case UriSerializer.Identifier:
                    return UriSerializer.Instance.ReadValue(data, session);
                case EmailAddressSerializer.Identifier:
                    return EmailAddressSerializer.Instance.ReadValue(data, session);
                case PhoneNumberSerializer.Identifier:
                    return PhoneNumberSerializer.Instance.ReadValue(data, session);
                case BandwithSerializer.Identifier:
                    session.ParsedValue.BandWiths = session.ParsedValue.BandWiths ?? new List<Bandwidth>();
                    session.ParsedValue.BandWiths.Add(BandwithSerializer.Instance.ReadValue(data));
                    return this.ReadValue;
                case RepeatTimeSerializer.Identifier:
                    session.ParsedValue.SDPRepeatTimes = session.ParsedValue.SDPRepeatTimes ?? new List<RepeatTime>();
                    session.ParsedValue.SDPRepeatTimes.Add(RepeatTimeSerializer.Instance.ReadValue(data));
                    return this.ReadValue;
                case SDPTimezonesSerializer.Identifier:
                    session.ParsedValue.TimeZones = SDPTimezonesSerializer.Instance.ReadValue(data);
                    return this.ReadValue;
                case EncriptionKeySerializer.Identifier:
                    session.ParsedValue.EncriptionKey = EncriptionKeySerializer.Instance.ReadValue(data);
                    return this.ReadValue;
                case AttributeSerializer.Identifier:
                    session.ParsedValue.Attributes = session.ParsedValue.Attributes ?? new List<string>();
                    session.ParsedValue.Attributes.Add(AttributeSerializer.Instance.ReadValue(data));
                    return this.ReadValue;
                case MediaSerializer.Identifier:
                    session.ParsedValue.MediaDescriptions = session.ParsedValue.MediaDescriptions ?? new List<MediaDescription>();
                    session.ParsedValue.MediaDescriptions.Add(MediaSerializer.Instance.ReadValue(data));
                    return MediaOptioalValueDeserializer.Instance.ReadValue;
                default:
                    throw new DeserializationException($"Unknown field {(char)data[0]}");
            }
        }
    }
}
