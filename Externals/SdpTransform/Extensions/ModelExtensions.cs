using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Mid ToMid(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Mid.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Mid
            {
                Id = tokens[0]
            };
        }

        public static MsidSemantic ToMsidSemantic(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                .Replace(MsidSemantic.Name, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var idList = new string[tokens.Length -1];
            for (int i = 0; i < idList.Length; i++)
                idList[i] = tokens[i + 1];
            return new MsidSemantic
            {
                Token = tokens[0],
                IdList = idList
            };
        }

        public static Fingerprint ToFingerprint(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Fingerprint.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Fingerprint
            {
                HashFunction = (HashFunction)Enum.Parse(typeof(HashFunction), tokens[0], true),
                HashValue = HexadecimalStringToByteArray(tokens[1].Replace(":", string.Empty))
            };

        }

        public static Group ToGroup(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Group.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var groupTokens = new string[tokens.Length - 1];
            for (int i = 1; i < tokens.Length; i++)
                groupTokens[i - 1] = tokens[i];
            return new Group
            {
                Type = tokens[0],
                Tokens = groupTokens
            };
        }

        public static Msid ToMsid(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Msid.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Msid
            {
                Id = tokens[0],
                AppData = tokens[1]
            };
        }

        public static Ssrc ToSsrc(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                .Replace(Ssrc.Name, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var attributeAndValue = tokens[1].Split(':');
            return new Ssrc
            {
                Id = uint.Parse(tokens[0]),
                Attribute = attributeAndValue[0],
                Value = attributeAndValue.Length > 1 ? attributeAndValue[1] : null
             };
        }

        public static SsrcGroup ToSsrcGroup(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                .Replace(SsrcGroup.Name, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var ssrcIds = new string[tokens.Length - 1];
            for (int i = 1; i < tokens.Length; i++)
                ssrcIds[i - 1] = tokens[i];
            return new SsrcGroup
            {
                Semantics = tokens[0],
                SsrcIds = ssrcIds
            };
        }

        public static Rid ToRid(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                .Replace(Rid.Name, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var subTokens = tokens[2].Split(';');
            var fmtList = subTokens.Where(st => st.StartsWith("pt=")).ToArray();
            var restrictions = subTokens.Skip(fmtList.Length).Take(subTokens.Length - fmtList.Length).ToArray();
            return new Rid
            {
                Id = tokens[0],
                Direction = (RidDirection)Enum.Parse(typeof(RidDirection), tokens[1], true),
                FmtList = fmtList,
                Restrictions = restrictions
            };
        }

        public static Rtpmap ToRtpmap(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Rtpmap.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var subTokens = tokens[1].Split('/');
            return new Rtpmap
            {
                PayloadType = int.Parse(tokens[0]),
                EncodingName = subTokens[0],
                ClockRate = int.Parse(subTokens[1]),
                Channels = subTokens.Length > 2 ? int.Parse(subTokens[2]) : null
            };
        }

        public static Fmtp ToFmtp(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Fmtp.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Fmtp
            {
                PayloadType = int.Parse(tokens[0]),
                Value = tokens[1],
            };
        }

        public static RtcpFb ToRtcpFb(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(RtcpFb.Name, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new RtcpFb
            {
                PayloadType = int.Parse(tokens[0]),
                Type = tokens[1],
                SubType = tokens.Length == 3 ? tokens[2] : null
            };
        }

        public static string ToAttributeString(this Candidate candidate, bool withAttributeCharacter = false)
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

        public static string ToAttributeString(this IceUfrag iceUfrag, bool withAttributeCharacter = false)
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
        
        public static string ToAttributeString(this IcePwd icePwd, bool withAttributeCharacter = false)
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

        public static string ToAttributeString(this IceOptions iceOptions, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(IceOptions.Name)
                .Append("");
            for (int i=0; i<iceOptions.Tags.Length; i++)
            {
                sb.Append(iceOptions.Tags[i]);
                if (i != iceOptions.Tags.Length - 1)
                    sb.Append(" ");
            }
            sb.Append(SdpSerializer.CRLF);
            
            return sb.ToString();
        }

        public static string ToAttributeString(this Mid mid, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(Mid.Name)
                .Append(mid.Id)
                .Append(SdpSerializer.CRLF);
            
            return sb.ToString();
        }

        public static string ToAttributeString(this MsidSemantic msidSemantic, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(MsidSemantic.Name)
                .Append(msidSemantic.Token)
                .Append(" ");
            for (int i= 0; i < msidSemantic.IdList.Length; i++)
            {
                sb.Append(msidSemantic.IdList[i]);
                if (i != msidSemantic.IdList.Length -1)
                    sb.Append(" ");
                sb.Append(" ");
            }
            sb.Append(SdpSerializer.CRLF);

            return sb.ToString();
        }

        public static string ToAttributeString(this Fingerprint fingerprint, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(Fingerprint.Name)
                .Append(fingerprint.HashFunction.DisplayName())
                .Append(" ")
                .Append(BitConverter.ToString(fingerprint.HashValue).Replace("-",":"))
                .Append(SdpSerializer.CRLF);

            return sb.ToString();
        }

        public static string ToAttributeString(this Group group, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(MsidSemantic.Name)
                .Append(group.Type)
                .Append(" ");
            for (int i = 0; i < group.Tokens.Length; i++)
            {
                sb.Append(group.Tokens[i]);
                if (i != group.Tokens.Length - 1)
                    sb.Append(" ");
            }
            sb.Append(SdpSerializer.CRLF);
            
            return sb.ToString();
        }

        public static string ToAttributeString(this Msid msid, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(Msid.Name)
                .Append(msid.Id)
                .Append(" ")
                .Append(msid.AppData)
                .Append(SdpSerializer.CRLF);

            return sb.ToString();
        }

        public static string ToAttributeString(this Ssrc ssrc, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(Ssrc.Name)
                .Append(ssrc.Id.ToString())
                .Append(" ")
                .Append(ssrc.Attribute);
            if (ssrc.Value is not null)
            {
                sb
                    .Append(":")
                    .Append(ssrc.Value);
            }
            sb.Append(SdpSerializer.CRLF);

            return sb.ToString();
        }

        public static string ToAttributeString(this SsrcGroup ssrcGroup, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(SsrcGroup.Name)
                .Append(ssrcGroup.Semantics)
                .Append(" ");
            for (int i = 0; i < ssrcGroup.SsrcIds.Length; i++)
            {
                sb.Append(ssrcGroup.SsrcIds[i]);
                if (i != ssrcGroup.SsrcIds.Length - 1)
                    sb.Append(" ");
            }
            sb.Append(SdpSerializer.CRLF);

            return sb.ToString();
        }

        public static string ToAttributeString(this Rid rid, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(Rid.Name)
                .Append(rid.Id)
                .Append(" ")
                .Append(rid.Direction.DisplayName());
            for (var i = 0; i < rid.FmtList.Length; i++)
            {
                sb.Append(rid.FmtList[i]);
                sb.Append(";");
            }
            for (var i = 0; i < rid.Restrictions.Length; i++)
            {
                sb.Append(rid.Restrictions[i]);
                if (i != rid.Restrictions.Length - 1)
                    sb.Append(";");
            }
            sb.Append(SdpSerializer.CRLF);

            return sb.ToString();
        }

        public static string ToAttributeString(this Rtpmap rtpmap, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(Rtpmap.Name)
                .Append(rtpmap.PayloadType.ToString())
                .Append(" ")
                .Append(rtpmap.EncodingName)
                .Append("/")
                .Append(rtpmap.ClockRate.ToString());
            if (rtpmap.Channels.HasValue)
            {
                sb
                    .Append("/")
                    .Append(rtpmap.Channels);
            }
            sb.Append(SdpSerializer.CRLF);

            return sb.ToString();

        }

        public static string ToAttributeString(this Fmtp fmtp, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(Fmtp.Name)
                .Append(fmtp.PayloadType.ToString())
                .Append(" ")
                .Append(fmtp.Value)
                .Append(SdpSerializer.CRLF);

            return sb.ToString();
        }

        public static string ToAttributeString(this RtcpFb rtcpFb, bool withAttributeCharacter = false)
        {
            StringBuilder sb = new();
            if (withAttributeCharacter)
                sb.Append(SdpSerializer.AttributeCharacter);
            sb
                .Append(RtcpFb.Name)
                .Append(rtcpFb.PayloadType.ToString())
                .Append(" ")
                .Append(rtcpFb.Type)
                .Append(SdpSerializer.CRLF);
            if (rtcpFb.SubType is not null)
            {
                sb
                    .Append(" ")
                    .Append(rtcpFb.SubType);
            }
            sb.Append(SdpSerializer.CRLF);

            return sb.ToString();

        }

        // Utility method.
        public static byte[] HexadecimalStringToByteArray(String hexadecimalString)
        {
            int length = hexadecimalString.Length;
            byte[] byteArray = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hexadecimalString.Substring(i, 2), 16);
            }
            return byteArray;
        }

    }
}
