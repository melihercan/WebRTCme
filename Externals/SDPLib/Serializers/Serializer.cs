using System;
using System.Buffers;
using System.Linq;

namespace SDPLib.Serializers
{
    class Serializer
    {
        public static readonly Serializer Instance = new Serializer();

        public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            session.ParsedValue = new SDP();
            session.ParsedValue.Version = VersionSerializer.Instance.ReadValue(data);
            return OriginSerializer.Instance.ReadValue;
        }

        public void WriteValue(IBufferWriter<byte> writer, SDP value)
        {
            VersionSerializer.Instance.WriteValue(writer, value.Version);
            OriginSerializer.Instance.WriteValue(writer, value.Origin);
            SessionNameSerializer.Instance.WriteValue(writer, value.SessionName);

            SessionInformationSerializer.Instance.WriteValue(writer, value.SessionInformation);
            UriSerializer.Instance.WriteValue(writer, value.Uri);

            if (value.EmailNumbers != null)
                foreach (var email in value.EmailNumbers)
                    EmailAddressSerializer.Instance.WriteValue(writer, email);

            if (value.PhoneNumbers != null)
                foreach (var phone in value.PhoneNumbers)
                    PhoneNumberSerializer.Instance.WriteValue(writer, phone);

            ConnectionDataSerializer.Instance.WriteValue(writer, value.ConnectionData);

            if (value.BandWiths != null)
                foreach (var bandwith in value.BandWiths)
                    BandwithSerializer.Instance.WriteValue(writer, bandwith);

            if (value.Attributes != null)
                foreach (var attr in value.Attributes)
                    AttributeSerializer.Instance.WriteValue(writer, attr);

            if (value.Timings != null)
                foreach (var timing in value.Timings)
                    TimingSerializer.Instance.WriteValue(writer, timing);

            if (value.SDPRepeatTimes != null)
                foreach (var repeatTime in value.SDPRepeatTimes)
                    RepeatTimeSerializer.Instance.WriteValue(writer, repeatTime);

            EncriptionKeySerializer.Instance.WriteValue(writer, value.EncriptionKey);

            if (value.MediaDescriptions == null || !value.MediaDescriptions.Any())
            {
                throw new SerializationException("At least one media description is required");
            }

            foreach (var media in value.MediaDescriptions)
            {
                MediaSerializer.Instance.WriteValue(writer, media);
                MediaTitleSerializer.Instance.WriteValue(writer, media.Title);
                ConnectionDataSerializer.Instance.WriteValue(writer, media.ConnectionInfo);

                if (value.BandWiths != null)
                    foreach (var bandwith in media.Bandwiths)
                        BandwithSerializer.Instance.WriteValue(writer, bandwith);

                EncriptionKeySerializer.Instance.WriteValue(writer, media.EncriptionKey);

                if (media.Attributes != null)
                    foreach (var attr in media.Attributes)
                        AttributeSerializer.Instance.WriteValue(writer, attr);
            }
        }
    }
}
