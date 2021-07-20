using System;
using System.Collections.Generic;
using System.Text;
using UtilmeSdpTransform;

namespace Utilme.SdpTransform
{
    public static class ModelExtensions
    {
        public static Candidate ToCandidate(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Candidate.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Candidate
            {
                Foundation = tokens[0],
                ComponentId = int.Parse(tokens[1]),
                Transport = (CandidateTransport)Enum.Parse(typeof(CandidateTransport), tokens[2], true),
                Priority = int.Parse(tokens[3]),
                ConnectionAddress = tokens[4],
                Port = int.Parse(tokens[5]),
                Type = (CandidateType)Enum.Parse(typeof(CandidateType), tokens[7], true),
                RelAddr = tokens[9],
                RelPort = int.Parse(tokens[11])
            };
        }

        public static IceUfrag ToIceUfrag(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(IceUfrag.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new IceUfrag
            {
                Ufrag = tokens[0]
            };
        }

        public static IcePwd ToIcePwd(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(IcePwd.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new IcePwd
            {
                Password = tokens[0]
            };
        }

        public static IceOptions ToIceOptions(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(IceOptions.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var tags = new string[tokens.Length];
            for (var i = 0; i < tokens.Length; i++)
                tags[i] = tokens[i];
            return new IceOptions
            {
                Tags = tags
            };
        }

        public static string ToString(this Candidate candidate, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(Candidate.Name)
                .Append(candidate.Foundation)
                .Append(" ")
                .Append(candidate.ComponentId.ToString())
                .Append(" ")
                .Append(candidate.Transport.DisplayName())
                .Append(" ")
                .Append(candidate.Priority.ToString())
                .Append(" ")
                .Append(candidate.ConnectionAddress)
                .Append(" ")
                .Append(Candidate.Typ)
                .Append(" ")
                .Append(candidate.Type.DisplayName())
                .Append(" ")
                .Append(Candidate.Raddr)
                .Append(" ")
                .Append(candidate.RelAddr)
                .Append(" ")
                .Append(Candidate.Rport)
                .Append(" ")
                .Append(candidate.RelPort)
                .Append(SdpSerializer.CRLF);
            return sb.ToString();
        }

        public static string ToString(this IceUfrag iceUfrag, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(IceUfrag.Name)
                .Append(iceUfrag.Ufrag)
                .Append(SdpSerializer.CRLF);
            return sb.ToString();
        }
        public static string ToString(this IcePwd icePwd, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(IcePwd.Name)
                .Append(icePwd.Password)
                .Append(SdpSerializer.CRLF);
            return sb.ToString();
        }

        public static string ToString(this IceOptions iceOptions, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            for (int i=0; i<iceOptions.Tags.Length; i++)
            {
                sb.Append(iceOptions.Tags[i]);
                if (i != iceOptions.Tags.Length - 1)
                    sb.Append(" ");
            }
            sb.Append(SdpSerializer.CRLF);
            return sb.ToString();
        }
    }
}
