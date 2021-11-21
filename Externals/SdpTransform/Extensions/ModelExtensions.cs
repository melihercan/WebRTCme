using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UtilmeSdpTransform;

namespace Utilme.SdpTransform
{
    public static class ModelExtensions
    {
        // https://www.iana.org/assignments/sdp-parameters/sdp-parameters.xhtml#sdp-parameters-12

        public static Sdp ToSdp(this string str)
        {
            try
            {
                Sdp sdp = new();

                var tokens = str.Split(new string[] { Sdp.CRLF }, StringSplitOptions.RemoveEmptyEntries);
                var idx = 0;

                // Session fields.
                foreach (var token in tokens)
                {
                    if (token.StartsWith(Sdp.ProtocolVersionIndicator))
                        sdp.ProtocolVersion = token.ToProtocolVersion();
                    else if (token.StartsWith(Sdp.OriginIndicator))
                        sdp.Origin = token.ToOrigin();
                    else if (token.StartsWith(Sdp.SessionNameIndicator))
                        sdp.SessionName = token.ToSessionName();
                    else if (token.StartsWith(Sdp.InformationIndicator))
                        sdp.SessionInformation = token.ToInformation();
                    else if (token.StartsWith(Sdp.UriIndicator))
                        sdp.Uri = token.ToUri();
                    else if (token.StartsWith(Sdp.EmailAddressIndicator))
                        sdp.EmailAddresses = token.ToEmailAddresses();
                    else if (token.StartsWith(Sdp.PhoneNumberIndicator))
                        sdp.PhoneNumbers = token.ToPhoneNumbers();
                    else if (token.StartsWith(Sdp.ConnectionDataIndicator))
                        sdp.ConnectionData = token.ToConnectionData();
                    else if (token.StartsWith(Sdp.BandwidthIndicator))
                    {
                        sdp.Bandwidths ??= new List<Bandwidth>();
                        sdp.Bandwidths.Add(token.ToBandwidth());
                    }
                    else if (token.StartsWith(Sdp.TimingIndicator))
                    {
                        sdp.Timings ??= new List<Timing>();
                        sdp.Timings.Add(token.ToTiming());
                    }
                    else if (token.StartsWith(Sdp.RepeatTimeIndicator))
                    {
                        sdp.RepeatTimes ??= new List<RepeatTime>();
                        sdp.RepeatTimes.Add(token.ToRepeatTime());
                    }
                    else if (token.StartsWith(Sdp.TimeZoneIndicator))
                        sdp.TimeZones = token.ToTimeZones();
                    else if (token.StartsWith(Sdp.EncryptionKeyIndicator))
                        sdp.EncryptionKey = token.ToEncryptionKey();
                    else if (token.StartsWith(Sdp.AttributeIndicator))
                    {
                        sdp.Attributes ??= new Attributes();
                        var attr = token.Substring(2);

                        // Binary attributes.
                        if (attr.StartsWith(Attributes.ExtmapAllowMixedLabel))
                            sdp.Attributes.ExtmapAllowMixed = true;
                        else if (attr.StartsWith(Attributes.IceLiteLabel))
                            sdp.Attributes.IceLite = true;

                        // Value attributes.
                        else if (attr.StartsWith(Group.Label))
                            sdp.Attributes.Group = attr.ToGroup();
                        else if (attr.StartsWith(MsidSemantic.Label))
                            sdp.Attributes.MsidSemantic = attr.ToMsidSemantic();
                        else if (attr.StartsWith(Fingerprint.Label))
                            sdp.Attributes.Fingerprint = attr.ToFingerprint();

                        else
                            Console.WriteLine($"==== SDP unsupported media description attribute:{attr}");
                        //throw new NotSupportedException($"Unknown SDP attribute for session: {attr}");

                    }
                    else if (token.StartsWith(Sdp.MediaDescriptionIndicator))
                        break;
                    else
                        Console.WriteLine($"==== SDP unsupported session field:{token}");
                    //throw new NotSupportedException($"Unknown SDP field for session: {token}");

                    idx++;
                }

                // Media description fields.
                tokens = tokens.Skip(idx).ToArray();
                sdp.MediaDescriptions = new List<MediaDescription>();

                MediaDescription md = null;

                foreach (var token in tokens)
                {
                    if (token.StartsWith(Sdp.MediaDescriptionIndicator))
                    {
                        if (md is not null)
                            sdp.MediaDescriptions.Add(md);
                        md = token.ToMediaDescription();
                    }
                    else if (token.StartsWith(Sdp.InformationIndicator))
                        md.Information = token.ToInformation();
                    else if (token.StartsWith(Sdp.ConnectionDataIndicator))
                        md.ConnectionData = token.ToConnectionData();
                    else if (token.StartsWith(Sdp.BandwidthIndicator))
                    {
                        md.Bandwidths ??= new List<Bandwidth>();
                        md.Bandwidths.Add(token.ToBandwidth());
                    }
                    else if (token.StartsWith(Sdp.EncryptionKeyIndicator))
                        md.EncryptionKey = token.ToEncryptionKey();
                    else if (token.StartsWith(Sdp.AttributeIndicator))
                    {
                        md.Attributes ??= new Attributes();
                        var attr = token.Substring(2);

                        // Binary attributes.
                        if (attr.StartsWith(Attributes.ExtmapAllowMixedLabel))
                            md.Attributes.ExtmapAllowMixed = true;
                        else if (attr.StartsWith(Attributes.RtcpMuxLabel))
                            md.Attributes.RtcpMux = true;
                        else if (attr.StartsWith(Attributes.RtcpRsizeLabel))
                            md.Attributes.RtcpRsize = true;
                        else if (attr.StartsWith(Attributes.SendRecvLabel))
                            md.Attributes.SendRecv = true;
                        else if (attr.StartsWith(Attributes.SendOnlyLabel))
                            md.Attributes.SendOnly = true;
                        else if (attr.StartsWith(Attributes.RecvOnlyLabel))
                            md.Attributes.RecvOnly = true;
                        else if (attr.StartsWith(Attributes.EndOfCandidatesLabel))
                            md.Attributes.EndOfCandidates = true;

                        // Value attributes.
                        else if (attr.StartsWith(Group.Label))
                            md.Attributes.Group = attr.ToGroup();
                        else if (attr.StartsWith(MsidSemantic.Label))
                            md.Attributes.MsidSemantic = attr.ToMsidSemantic();
                        else if (attr.StartsWith(Mid.Label))
                            md.Attributes.Mid = attr.ToMid();
                        else if (attr.StartsWith(Msid.Label))
                            md.Attributes.Msid = attr.ToMsid();
                        else if (attr.StartsWith(IceUfrag.Label))
                            md.Attributes.IceUfrag = attr.ToIceUfrag();
                        else if (attr.StartsWith(IcePwd.Label))
                            md.Attributes.IcePwd = attr.ToIcePwd();
                        else if (attr.StartsWith(IceOptions.Label))
                            md.Attributes.IceOptions = attr.ToIceOptions();
                        else if (attr.StartsWith(Fingerprint.Label))
                            md.Attributes.Fingerprint = attr.ToFingerprint();
                        else if (attr.StartsWith(Rtcp.Label))
                            md.Attributes.Rtcp = attr.ToRtcp();
                        else if (attr.StartsWith(Setup.Label))
                            md.Attributes.Setup = attr.ToSetup();
                        else if (attr.StartsWith(SctpPort.Label))
                            md.Attributes.SctpPort = attr.ToSctpPort();
                        else if (attr.StartsWith(MaxMessageSize.Label))
                            md.Attributes.MaxMessageSize = attr.ToMaxMessageSize();
                        else if (attr.StartsWith(Simulcast.Label))
                            md.Attributes.Simulcast = attr.ToSimulcast();
                        else if (attr.StartsWith(Candidate.Label))
                        {
                            md.Attributes.Candidates ??= new List<Candidate>();
                            md.Attributes.Candidates.Add(attr.ToCandidate());
                        }
                        else if (attr.StartsWith(Ssrc.Label))
                        {
                            md.Attributes.Ssrcs ??= new List<Ssrc>();
                            md.Attributes.Ssrcs.Add(attr.ToSsrc());
                        }
                        else if (attr.StartsWith(SsrcGroup.Label))
                        {
                            md.Attributes.SsrcGroups ??= new List<SsrcGroup>();
                            md.Attributes.SsrcGroups.Add(attr.ToSsrcGroup());
                        }
                        else if (attr.StartsWith(Rid.Label))
                        {
                            md.Attributes.Rids ??= new List<Rid>();
                            md.Attributes.Rids.Add(attr.ToRid());
                        }
                        else if (attr.StartsWith(Rtpmap.Label))
                        {
                            md.Attributes.Rtpmaps ??= new List<Rtpmap>();
                            md.Attributes.Rtpmaps.Add(attr.ToRtpmap());
                        }
                        else if (attr.StartsWith(Fmtp.Label))
                        {
                            md.Attributes.Fmtps ??= new List<Fmtp>();
                            md.Attributes.Fmtps.Add(attr.ToFmtp());
                        }
                        else if (attr.StartsWith(RtcpFb.Label))
                        {
                            md.Attributes.RtcpFbs ??= new List<RtcpFb>();
                            md.Attributes.RtcpFbs.Add(attr.ToRtcpFb());
                        }
                        else if (attr.StartsWith(Extmap.Label))
                        {
                            md.Attributes.Extmaps ??= new List<Extmap>();
                            md.Attributes.Extmaps.Add(attr.ToExtmap());
                        }

                        else
                            Console.WriteLine($"==== SDP unsupported media description attribute:{attr}");
                        //throw new NotSupportedException($"Unknown SDP attribute for media description: {attr}");

                    }
                    else
                        Console.WriteLine($"==== SDP unsupported media description field:{token}");
                    //throw new NotSupportedException($"Unknown SDP field for media description: {token}");

                }

                if (md is not null)
                    sdp.MediaDescriptions.Add(md);

                return sdp;
            }
            catch (Exception ex)
            {
                var m = ex.Message;
                return null;
            }
        }

        public static string ToText(this Sdp sdp)
        {
            StringBuilder sb = new();

            // Session fields.
            sb.Append(ToProtocolVersionText(sdp.ProtocolVersion));
            sb.Append(sdp.Origin.ToText());
            sb.Append(ToSessionNameText(sdp.SessionName));
            if (sdp.SessionInformation is not null)
                sb.Append(ToInformationText(sdp.SessionInformation));
            if (sdp.Uri is not null)
                sb.Append(sdp.Uri.ToText());
            if (sdp.EmailAddresses is not null)
                sb.Append(sdp.EmailAddresses.ToEmailAddressesText());
            if (sdp.PhoneNumbers is not null)
                sb.Append(sdp.PhoneNumbers.ToPhoneNumbersText());
            if (sdp.ConnectionData is not null)
                sb.Append(sdp.ConnectionData.ToText());
            if (sdp.Bandwidths is not null)
                foreach (var b in sdp.Bandwidths)
                    sb.Append(b.ToText());
            foreach (var t in sdp.Timings)
                sb.Append(t.ToText());
            if (sdp.RepeatTimes is not null)
                foreach (var r in sdp.RepeatTimes)
                    sb.Append(r.ToText());
            if (sdp.TimeZones is not null)
                sb.Append(sdp.TimeZones.ToText());
            if (sdp.EncryptionKey is not null)
                sb.Append(sdp.EncryptionKey.ToText());
            
            // Session binary attributes.
            if (sdp.Attributes.ExtmapAllowMixed.HasValue)
                sb.Append($"{Sdp.AttributeIndicator}{Attributes.ExtmapAllowMixedLabel}{Sdp.CRLF}");
            if (sdp.Attributes.IceLite.HasValue)
                sb.Append($"{Sdp.AttributeIndicator}{Attributes.IceLiteLabel}{Sdp.CRLF}");

            // Session value attributes.
            if (sdp.Attributes.Group is not null)
                sb.Append(sdp.Attributes.Group.ToText());
            if (sdp.Attributes.MsidSemantic is not null)
                sb.Append(sdp.Attributes.MsidSemantic.ToText());
            if (sdp.Attributes.Fingerprint is not null)
                sb.Append(sdp.Attributes.Fingerprint.ToText());

            // Media description fields.
            foreach (var md in sdp.MediaDescriptions)
            {
                sb.Append(md.ToText());

                if (md.Information is not null)
                    sb.Append(md.Information.ToInformationText());
                if (md.ConnectionData is not null)
                    sb.Append(md.ConnectionData.ToText());
                if (md.Bandwidths is not null)
                    foreach (var b in md.Bandwidths)
                        sb.Append(b.ToText());
                if (md.EncryptionKey is not null)
                    sb.Append(md.EncryptionKey.ToText());

                // Media description binary attributes.
                if (md.Attributes.ExtmapAllowMixed.HasValue)
                    sb.Append($"{Sdp.AttributeIndicator}{Attributes.ExtmapAllowMixedLabel}{Sdp.CRLF}");
                if (md.Attributes.RtcpMux.HasValue)
                    sb.Append($"{Sdp.AttributeIndicator}{Attributes.RtcpMuxLabel}{Sdp.CRLF}");
                if (md.Attributes.RtcpRsize.HasValue)
                    sb.Append($"{Sdp.AttributeIndicator}{Attributes.RtcpRsizeLabel}{Sdp.CRLF}");
                if (md.Attributes.SendRecv.HasValue)
                    sb.Append($"{Sdp.AttributeIndicator}{Attributes.SendRecvLabel}{Sdp.CRLF}");
                if (md.Attributes.SendOnly.HasValue)
                    sb.Append($"{Sdp.AttributeIndicator}{Attributes.SendOnlyLabel}{Sdp.CRLF}");
                if (md.Attributes.RecvOnly.HasValue)
                    sb.Append($"{Sdp.AttributeIndicator}{Attributes.RecvOnlyLabel}{Sdp.CRLF}");

                // Media description value attributes.
                if (md.Attributes.Group is not null)
                    sb.Append(md.Attributes.Group.ToText());
                if (md.Attributes.MsidSemantic is not null)
                    sb.Append(md.Attributes.MsidSemantic.ToText());
                if (md.Attributes.Mid is not null)
                    sb.Append(md.Attributes.Mid.ToText());
                if (md.Attributes.Msid is not null)
                    sb.Append(md.Attributes.Msid.ToText());
                if (md.Attributes.IceUfrag is not null)
                    sb.Append(md.Attributes.IceUfrag.ToText());
                if (md.Attributes.IcePwd is not null)
                    sb.Append(md.Attributes.IcePwd.ToText());
                if (md.Attributes.IceOptions is not null)
                    sb.Append(md.Attributes.IceOptions.ToText());
                if (md.Attributes.Fingerprint is not null)
                    sb.Append(md.Attributes.Fingerprint.ToText());
                if (md.Attributes.Rtcp is not null)
                    sb.Append(md.Attributes.Rtcp.ToText());
                if (md.Attributes.Setup is not null)
                    sb.Append(md.Attributes.Setup.ToText());
                if (md.Attributes.SctpPort is not null)
                    sb.Append(md.Attributes.SctpPort.ToText());
                if (md.Attributes.MaxMessageSize is not null)
                    sb.Append(md.Attributes.MaxMessageSize.ToText());
                if (md.Attributes.Simulcast is not null)
                    sb.Append(md.Attributes.Simulcast.ToText());
                if (md.Attributes.Candidates is not null)
                    foreach (var c in md.Attributes.Candidates)
                        sb.Append(c.ToText());
                if (md.Attributes.EndOfCandidates.HasValue)
                    sb.Append($"{Sdp.AttributeIndicator}{Attributes.EndOfCandidatesLabel}{Sdp.CRLF}");
                if (md.Attributes.Ssrcs is not null)
                    foreach (var s in md.Attributes.Ssrcs)
                        sb.Append(s.ToText());
                if (md.Attributes.SsrcGroups is not null)
                    foreach (var sg in md.Attributes.SsrcGroups)
                        sb.Append(sg.ToText());
                if (md.Attributes.Rids is not null)
                    foreach (var r in md.Attributes.Rids)
                        sb.Append(r.ToText());
                if (md.Attributes.Rtpmaps is not null)
                    foreach (var r in md.Attributes.Rtpmaps)
                        sb.Append(r.ToText());
                if (md.Attributes.Fmtps is not null)
                    foreach (var f in md.Attributes.Fmtps)
                        sb.Append(f.ToText());
                if (md.Attributes.RtcpFbs is not null)
                    foreach (var r in md.Attributes.RtcpFbs)
                        sb.Append(r.ToText());
                if (md.Attributes.Extmaps is not null)
                    foreach (var e in md.Attributes.Extmaps)
                        sb.Append(e.ToText());
            }

            return sb.ToString();
        }

        public static int ToProtocolVersion(this string str)
        {
            var token = str
                .Replace(Sdp.ProtocolVersionIndicator, string.Empty)
                .Replace(Sdp.CRLF, string.Empty);
            return int.Parse(token);
        }

        public static string ToProtocolVersionText(this int version) =>
            $"{Sdp.ProtocolVersionIndicator}{version}{Sdp.CRLF}";

        public static Origin ToOrigin(this string str)
        {
            var tokens = str
                .Replace(Sdp.OriginIndicator, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Origin
            {
                UserName = tokens[0],
                SessionId = ulong.Parse(tokens[1]),
                SessionVersion = uint.Parse(tokens[2]),
                NetType = tokens[3].EnumFromDisplayName<NetType>(),
                AddrType = tokens[4].EnumFromDisplayName<AddrType>(),
                UnicastAddress = tokens[5]
            };
        }

        public static string ToText(this Origin origin) =>
            $"{Sdp.OriginIndicator}{origin.UserName} {origin.SessionId} {origin.SessionVersion} " +
                $"{origin.NetType.DisplayName()} {origin.AddrType.DisplayName()} {origin.UnicastAddress}" +
                $"{Sdp.CRLF}";

        public static string ToSessionName(this string str)
        {
            var token = str
                .Replace(Sdp.SessionNameIndicator, string.Empty)
                .Replace(Sdp.CRLF, string.Empty);
            return token;
        }

        public static string ToSessionNameText(this string name) =>
            $"{Sdp.SessionNameIndicator}{name}{Sdp.CRLF}";


        public static string ToInformation(this string str)
        {
            var token = str
                .Replace(Sdp.InformationIndicator, string.Empty)
                .Replace(Sdp.CRLF, string.Empty);
            return token;
        }

        public static string ToInformationText(this string info) =>
            $"{Sdp.InformationIndicator}{info}{Sdp.CRLF}";

        public static Uri ToUri(this string str)
        {
            var token = str
                .Replace(Sdp.UriIndicator, string.Empty)
                .Replace(Sdp.CRLF, string.Empty);
            return new Uri(token);
        }

        public static string ToText(this Uri uri) =>
            $"{Sdp.UriIndicator}{uri}{Sdp.CRLF}";

        public static List<string> ToEmailAddresses(this string str)
        {
            var token = str
                .Replace(Sdp.EmailAddressIndicator, string.Empty)
                .Replace(Sdp.CRLF, string.Empty);

            List<string> emails = new();

            // Email formats:
            //  x.y@z.org
            //  x.y@z.org (Name Surname)
            //  Name Surname <x.y@z.org>
            var groupA = token.Split(new char[] { ')', '>' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var a in groupA)
            {
                if (a.Contains("("))
                {
                    var groupB = a.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
                    var id = groupB[1];
                    var groupC = groupB[0].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var emailWithId = $"{groupC[groupC.Length - 1]} ({id})";
                    if (groupC.Length > 1)
                    {
                        var groupD = groupC.Take(groupC.Length - 1).ToArray();
                        foreach (var d in groupD)
                            emails.Add(d);
                    }
                    emails.Add(emailWithId);
                }
                else if (a.Contains("<"))
                {
                    var groupB = a.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
                    var groupC = groupB[0].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var numPlainEmails = groupC.Where(g => g.Contains('@')).Count();
                    var plainEmails = groupC.Take(numPlainEmails);
                    var id = string.Join(" ", 
                        groupC.Skip(numPlainEmails).Take(groupC.Count() - numPlainEmails).ToArray());
                    var email = groupB[1];
                    var emailWithId = $"{id} <{email}>";
                    foreach (var plainEmail in plainEmails)
                        emails.Add(plainEmail);
                    emails.Add(emailWithId);
                }
                else
                {
                    var groupB = a.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var b in groupB)
                        emails.Add(b);
                }
            }

            return emails;
        }

        public static string ToEmailAddressesText(this IList<string> emails) =>
            $"{Sdp.EmailAddressIndicator}{string.Join(" ", emails)}{Sdp.CRLF}";

        public static List<string> ToPhoneNumbers(this string str)
        {
            var token = str
                 .Replace(Sdp.PhoneNumberIndicator, string.Empty)
                 .Replace(Sdp.CRLF, string.Empty);

            List<string> phones = new();

            // Phone formats:
            //  +x.y@z.org
            //  x.y@z.org (Name Surname)
            //  Name Surname <x.y@z.org>
            var groupA = token.Split(new char[] { ')', '>' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var a in groupA)
            {
                if (a.Contains("("))
                {
                    var groupB = a.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
                    var id = groupB[1];
                    var groupC = groupB[0].Trim().Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                    var phoneWithId = $"+{groupC[groupC.Length - 1].Trim()} ({id})";
                    if (groupC.Length > 1)
                    {
                        var groupD = groupC.Take(groupC.Length - 1).ToArray();
                        foreach (var d in groupD)
                            phones.Add($"+{d.Trim()}");
                    }
                    phones.Add(phoneWithId);
                }
                else if (a.Contains("<"))
                {
                    var groupB = a.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
                    var groupC = Regex.Matches(groupB[0].Trim(), @"\+[\d -]+");
                    var numPlainPhones = groupC.Count - 1;
                    var lenPhones = 0;
//                    var plainPhones = new string[numPlainPhones];
                    for (var i = 0; i < numPlainPhones; i++)
                    {
                        lenPhones += groupC[i].Length;
                        phones.Add(groupC[i].Value.Trim());
                    }
                    lenPhones += groupC[numPlainPhones].Length;
                    var id = groupB[0].Trim().Substring(lenPhones);
                    var phone = groupC[1].Value.Trim();
                    var phoneWithId = $"{id} <{phone.Trim()}>";
                    phones.Add(phoneWithId);
                }
                else
                {
                    var groupB = a.Trim().Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var b in groupB)
                        phones.Add($"+{b.Trim()}");
                }
            }

            return phones;
        }

        public static string ToPhoneNumbersText(this IList<string> phones) =>
            $"{Sdp.PhoneNumberIndicator}{string.Join(" ", phones)}{Sdp.CRLF}";

        public static ConnectionData ToConnectionData(this string str)
        {
            var tokens = str
                 .Replace(Sdp.ConnectionDataIndicator, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new ConnectionData
            {
                NetType = tokens[0].EnumFromDisplayName<NetType>(),
                AddrType = tokens[1].EnumFromDisplayName<AddrType>(),
                ConnectionAddress = tokens[2]
            };
        }

        public static string ToText(this ConnectionData connectionData) =>
            $"{Sdp.ConnectionDataIndicator}{connectionData.NetType.DisplayName()} " +
                $"{connectionData.AddrType.DisplayName()} {connectionData.ConnectionAddress}" +
                $"{Sdp.CRLF}";

        public static Bandwidth ToBandwidth(this string str)
        {
            var tokens = str
                 .Replace(Sdp.BandwidthIndicator, string.Empty)
                 .Split(new char[] { ' ', ':', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Bandwidth
            {
                Type = tokens[0].EnumFromDisplayName<BandwidthType>(),
                Value = int.Parse(tokens[1])
            };
        }

        public static string ToText(this Bandwidth bandwidth) =>
            $"{Sdp.BandwidthIndicator}{bandwidth.Type} {bandwidth.Value}" +
                $"{Sdp.CRLF}";

        public static Timing ToTiming(this string str)
        {
            var tokens = str
                 .Replace(Sdp.TimingIndicator, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Timing
            {
                StartTime = new DateTime(1900, 1, 1) + TimeSpan.FromSeconds(ulong.Parse(tokens[0])),
                StopTime = new DateTime(1900, 1, 1) + TimeSpan.FromSeconds(ulong.Parse(tokens[1]))
            };
        }

        public static string ToText(this Timing timing) =>
            $"{Sdp.TimingIndicator}{(timing.StartTime - new DateTime(1900, 1, 1)).TotalSeconds} " +
                $"{(timing.StopTime - new DateTime(1900, 1, 1)).TotalSeconds}" +
                $"{Sdp.CRLF}";

        public static RepeatTime ToRepeatTime(this string str)
        {
            var tokens = str
                 .Replace(Sdp.RepeatTimeIndicator, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var offsetsStr = tokens.Skip(2);
            var offsets = new List<TimeSpan>();
            foreach (var offsetStr in offsetsStr)
                offsets.Add(TimeSpan.FromSeconds(Regex.IsMatch(offsetStr, @"[dhms]$") ?
                    ToSeconds(offsetStr) : double.Parse(offsetStr)));

            return new RepeatTime
            {
                RepeatInterval = TimeSpan.FromSeconds(Regex.IsMatch(tokens[0], @"[dhms]$") ?
                    ToSeconds(tokens[0]) : double.Parse(tokens[0])),
                ActiveDuration = TimeSpan.FromSeconds(Regex.IsMatch(tokens[1], @"[dhms]$") ?
                    ToSeconds(tokens[1]) : double.Parse(tokens[1])),
                OffsetsFromStartTime = offsets
            };
        }

        public static string ToText(this RepeatTime repeatTime) =>
            $"{Sdp.RepeatTimeIndicator}{repeatTime.RepeatInterval.TotalSeconds} " +
                $"{repeatTime.ActiveDuration.TotalSeconds} " +
                $"{string.Join(" ", repeatTime.OffsetsFromStartTime.Select(o => o.TotalSeconds).ToArray())}" +
                $"{Sdp.CRLF}";

        public static List<TimeZone> ToTimeZones(this string str)
        {
            var tokens = str
                .Replace(Sdp.TimeZoneIndicator, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length % 2 != 0)
                throw new FormatException("Timezones should be specified in pairs");
            var pairs = tokens
                .Select((token, idx) => new { idx, token })
                .GroupBy(p => p.idx / 2, p => p.token);

            List<TimeZone> timeZones = new();

            foreach (var pair in pairs)
            {
                var array = pair.ToArray();
                timeZones.Add(new TimeZone 
                { 
                    AdjustmentTime = new DateTime(1900, 1, 1) + TimeSpan.FromSeconds(double.Parse(array[0])),
                    Offset = TimeSpan.FromSeconds(Regex.IsMatch(array[1], @"[dhms]$") ?
                        ToSeconds(array[1]) : double.Parse(array[1]))
                });
            }

            return timeZones;
        }

        public static string ToText(this IList<TimeZone> timeZones) =>
            $"{Sdp.TimeZoneIndicator}" +
                $"{string.Join(" ", timeZones.Select(z => (z.AdjustmentTime - new DateTime(1900, 1, 1)).TotalSeconds.ToString() + " " + z.Offset.TotalSeconds.ToString()))}" +
                $"{Sdp.CRLF}";

        public static EncryptionKey ToEncryptionKey(this string str)
        {
            var tokens = str
                .Replace(Sdp.EncryptionKeyIndicator, string.Empty)
                .Split(new char[] { ':', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return new EncryptionKey
            {
                Method = tokens[0].EnumFromDisplayName<EncryptionKeyMethod>(),
                Value = tokens[1]
            };
        }

        public static string ToText(this EncryptionKey encryptionKey) =>
            $"{Sdp.EncryptionKeyIndicator}{encryptionKey.Method.DisplayName()}:{encryptionKey.Value}" +
                $"{Sdp.CRLF}";


        public static MediaDescription ToMediaDescription(this string str)
        {
            var tokens = str
                .Replace(Sdp.MediaDescriptionIndicator, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new MediaDescription
            {
                Media = tokens[0].EnumFromDisplayName<MediaType>(),
                Port = int.Parse(tokens[1]),
                Proto = tokens[2],
                Fmts = tokens.Skip(3).ToList()
            };
        }

        public static string ToText(this MediaDescription mediaDescription) =>
            $"{Sdp.MediaDescriptionIndicator}" +
                $"{mediaDescription.Media.DisplayName()} {mediaDescription.Port} {mediaDescription.Proto} " +
                $"{string.Join(" ", mediaDescription.Fmts.ToArray())}" +
                $"{Sdp.CRLF}";

        
        // Attributes.

        public static Group ToGroup(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(Group.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Group
            {
                Semantics = tokens[0].EnumFromDisplayName<GroupSemantics>(),
                SemanticsExtensions = tokens.Skip(1).ToArray()
            };
        }

        public static string ToText(this Group group) =>
            $"{Sdp.AttributeIndicator}{Group.Label}{group.Semantics.DisplayName()} " +
                $"{string.Join(" ", group.SemanticsExtensions)}" +
                $"{Sdp.CRLF}";

        public static MsidSemantic ToMsidSemantic(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(MsidSemantic.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new MsidSemantic
            {
                Token = tokens[0],
                IdList = tokens.Length > 1 ? tokens.Skip(1).ToArray() : null
            };
        }
        
        public static string ToText(this MsidSemantic msidSemantic) =>
            $"{Sdp.AttributeIndicator}{MsidSemantic.Label}" +
                $"{msidSemantic.Token} " +
                $"{(msidSemantic.IdList is not null ? string.Join(" ", msidSemantic.IdList) : string.Empty)}" +
                $"{Sdp.CRLF}";

        public static Mid ToMid(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(Mid.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Mid
            {
                Id = tokens[0]
            };
        }

        public static string ToText(this Mid mid) =>
            $"{Sdp.AttributeIndicator}{Mid.Label}" +
                $"{mid.Id}" +
                $"{Sdp.CRLF}";

        public static Msid ToMsid(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(Msid.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Msid
            {
                Id = tokens[0],
                AppData = tokens[1]
            };
        }
        
        public static string ToText(this Msid msid) =>
            $"{Sdp.AttributeIndicator}{Msid.Label}" +
                $"{msid.Id} {msid.AppData}" +
                $"{Sdp.CRLF}";


        public static IceUfrag ToIceUfrag(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(IceUfrag.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new IceUfrag
            {
                Ufrag = tokens[0]
            };
        }

        public static string ToText(this IceUfrag iceUfrag) =>
            $"{Sdp.AttributeIndicator}{IceUfrag.Label}" +
                $"{iceUfrag.Ufrag}" +
                $"{Sdp.CRLF}";

        public static IcePwd ToIcePwd(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(IcePwd.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new IcePwd
            {
                Password = tokens[0]
            };
        }

        public static string ToText(this IcePwd icePwd) =>
            $"{Sdp.AttributeIndicator}{IcePwd.Label}" +
                $"{icePwd.Password}" +
                $"{Sdp.CRLF}";

        public static IceOptions ToIceOptions(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(IceOptions.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new IceOptions
            {
                Tags = tokens
            };
        }

        public static string ToText(this IceOptions iceOptions) =>
            $"{Sdp.AttributeIndicator}{IceOptions.Label}" +
                $"{string.Join(" ", iceOptions.Tags)}" +
                $"{Sdp.CRLF}";

        public static Fingerprint ToFingerprint(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(Fingerprint.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Fingerprint
            {
                HashFunction = tokens[0].EnumFromDisplayName<HashFunction>(), 
                HashValue = HexadecimalStringToByteArray(tokens[1].Replace(":", string.Empty))
            };
        }

        public static string ToText(this Fingerprint fingerprint) =>
            $"{Sdp.AttributeIndicator}{Fingerprint.Label}" +
                $"{fingerprint.HashFunction.DisplayName()} " +
                $"{BitConverter.ToString(fingerprint.HashValue).Replace("-", ":")}" +
                $"{Sdp.CRLF}";

        public static Rtcp ToRtcp(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(Rtcp.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Rtcp
            {
                Port = int.Parse(tokens[0]),
                NetType = tokens.Length > 3 ? tokens[1].EnumFromDisplayName<NetType>() : null,
                AddrType = tokens.Length > 3 ? tokens[2].EnumFromDisplayName<AddrType>() : null,
                ConnectionAddress = tokens.Length > 3 ? tokens[3] : null
            };
        }

        public static string ToText(this Rtcp rtcp) =>
            $"{Sdp.AttributeIndicator}{Rtcp.Label}" +
                $"{rtcp.Port}{(rtcp.NetType.HasValue ? $" {rtcp.NetType.DisplayName()}" : string.Empty)}" +
                $"{(rtcp.AddrType.HasValue ? $" {rtcp.AddrType.DisplayName()}" : string.Empty)}" +
                $"{(rtcp.ConnectionAddress is not null ? $" {rtcp.ConnectionAddress}" : string.Empty)}" +
                $"{Sdp.CRLF}";

        public static Setup ToSetup(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(Setup.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Setup
            {
                Role = tokens[0].EnumFromDisplayName<SetupRole>()
            };
        }

        public static string ToText(this Setup setup) =>
            $"{Sdp.AttributeIndicator}{Setup.Label}" +
                $"{setup.Role.DisplayName()}" +
                $"{Sdp.CRLF}";


        public static SctpPort ToSctpPort(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(SctpPort.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new SctpPort
            {
                Port = int.Parse(tokens[0])
            };
        }

        public static string ToText(this SctpPort sctpPort) =>
            $"{Sdp.AttributeIndicator}{SctpPort.Label}" +
                $"{sctpPort.Port}" +
                $"{Sdp.CRLF}";

        public static MaxMessageSize ToMaxMessageSize(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(MaxMessageSize.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new MaxMessageSize
            {
                Size = int.Parse(tokens[0])
            };
        }

        public static string ToText(this MaxMessageSize size) =>
            $"{Sdp.AttributeIndicator}{MaxMessageSize.Label}" +
                $"{size.Size}" +
                $"{Sdp.CRLF}";



        public static Candidate ToCandidate(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(Candidate.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var candidate = new Candidate
            {
                Foundation = tokens[0],
                ComponentId = int.Parse(tokens[1]),
                Transport = tokens[2].EnumFromDisplayName<CandidateTransport>(),
                Priority = int.Parse(tokens[3]),
                ConnectionAddress = tokens[4],
                Port = int.Parse(tokens[5]),
                Type = tokens[7].EnumFromDisplayName<CandidateType>(),
            };
            var idx = 8;
            if (tokens.Length >= 8 && tokens[8] == Candidate.Raddr)
            {
                candidate.RelAddr = tokens[9];
                candidate.RelPort = int.Parse(tokens[11]);
                idx += 4;
            }

            var extensions = new List <(string, string)>(); 
            while (tokens.Length > idx)
            {
                extensions.Add((tokens[idx++], tokens[idx++]));
            }
            if (extensions.Count > 0)
                candidate.Extensions = extensions.ToArray();
            return candidate;
        }

        public static string ToText(this Candidate candidate) =>
            $"{Sdp.AttributeIndicator}{Candidate.Label}" +
                $"{candidate.Foundation} {candidate.ComponentId} {candidate.Transport.DisplayName()} " +
                $"{candidate.Priority} {candidate.ConnectionAddress} {candidate.Port} {Candidate.Typ} " +
                $"{candidate.Type.DisplayName()}" +
                $"{(candidate.RelAddr is not null ? $" {Candidate.Raddr} {candidate.RelAddr}" : string.Empty)}" +
                $"{(candidate.RelPort.HasValue ? $" {Candidate.Rport} {candidate.RelPort}" : string.Empty)}" +
                $"{(candidate.Extensions is null ? string.Empty : string.Join("", candidate.Extensions.Select(pair => " " + pair.Item1 + " " + pair.Item2).ToArray()))}" +
                $"{Sdp.CRLF}";

        public static Ssrc ToSsrc(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(Ssrc.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var attributeAndValue = tokens[1].Split(':');
            return new Ssrc
            {
                Id = uint.Parse(tokens[0]),
                Attribute = attributeAndValue[0],
                Value = attributeAndValue.Length > 1 ? attributeAndValue[1] : null
             };
        }

        public static string ToText(this Ssrc ssrc) =>
            $"{Sdp.AttributeIndicator}{Ssrc.Label}" +
                $"{ssrc.Id} {ssrc.Attribute}{(ssrc.Value == null ? string.Empty : ":" + ssrc.Value)}" +
                $"{Sdp.CRLF}";
        

        public static SsrcGroup ToSsrcGroup(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(SsrcGroup.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new SsrcGroup
            {
                Semantics = tokens[0],
                SsrcIds = tokens.Skip(1).ToArray()
            };
        }

        public static string ToText(this SsrcGroup ssrcGroup) =>
            $"{Sdp.AttributeIndicator}{SsrcGroup.Label}" +
                $"{ssrcGroup.Semantics} {string.Join(" ", ssrcGroup.SsrcIds)}" +
                $"{Sdp.CRLF}";

        public static Rid ToRid(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(Rid.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Rid rid = new()
            {
                Id = tokens[0],
                Direction = tokens[1].EnumFromDisplayName<RidDirection>(),
            };

            if (tokens.Length > 2)
            {
                var subTokens = tokens[2].Split(';');

                var fmtSubToken = subTokens.SingleOrDefault(s => s.StartsWith("pt="));
                if (fmtSubToken is not null)
                {
                    var fmtList = fmtSubToken
                        .Replace("pt=", string.Empty)
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    rid.FmtList = fmtList;
                }

                var restrictions = subTokens.Where(s => !s.StartsWith("pt=")).ToArray();
                if (restrictions.Length > 0)
                    rid.Restrictions = restrictions;
            }

            return rid;
        }

        public static string ToText(this Rid rid) =>
            $"{Sdp.AttributeIndicator}{Rid.Label}" +
                $"{rid.Id} {rid.Direction.DisplayName()} " +
                $"{(rid.FmtList is null ? string.Empty : "pt=" + string.Join(",",rid.FmtList))}" +
                $"{(rid.Restrictions is null ? string.Empty : ";" + string.Join(";", rid.Restrictions))}" +
                $"{Sdp.CRLF}";

        public static Simulcast ToSimulcast(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(Simulcast.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var subTokens = tokens[1].Split(';');
            return new Simulcast
            {
                Direction = tokens[0].EnumFromDisplayName<RidDirection>(),
                IdList = subTokens
            };

        }

        public static string ToText(this Simulcast simulcast) =>
            $"{Sdp.AttributeIndicator}{Simulcast.Label}" +
                $"{simulcast.Direction.DisplayName()} " +
                $"{string.Join(";", simulcast.IdList)}" +
                $"{Sdp.CRLF}";

        public static Rtpmap ToRtpmap(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(Rtpmap.Label, string.Empty)
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

        public static string ToText(this Rtpmap rtpmap) =>
            $"{Sdp.AttributeIndicator}{Rtpmap.Label}" +
                $"{rtpmap.PayloadType} {rtpmap.EncodingName}/{rtpmap.ClockRate}" +
                $"{(rtpmap.Channels.HasValue ? "/" + rtpmap.Channels : string.Empty)}" +
                $"{Sdp.CRLF}";

        public static Fmtp ToFmtp(this string str)
        {
            var tokens = str
                 .Replace(Sdp.AttributeIndicator, string.Empty)
                 .Replace(Fmtp.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Fmtp
            {
                PayloadType = int.Parse(tokens[0]),
                Value = tokens[1],
            };
        }

        // Dictionary
        //  {
        //      { "level-asymmetry-allowed", "1" },
        //      { "packetization-mode", "0" },
        //      { "profile-level-id", "42e01f" }
        //  }
        // Fmtp
        // {
        //  PayloadType = 108,
        //  Value = "level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f"
        // }
        // a=fmtp:108 level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f
        public static Fmtp ToFmtp(this Dictionary<string, object> dictionary, int payloadType)
        {
            Fmtp fmtp = new()
            {
                PayloadType = payloadType,
                Value = string.Empty
            };

            foreach (var key in dictionary.Keys)
            {
                if (!string.IsNullOrEmpty(fmtp.Value))
                    fmtp.Value += ";";
                fmtp.Value += $"{key}={dictionary[key]}";
            }

            return fmtp;
        }

        public static string ToText(this Fmtp fmtp) =>
                $"{Sdp.AttributeIndicator}{Fmtp.Label}" +
                    $"{fmtp.PayloadType} {fmtp.Value}" +
                    $"{Sdp.CRLF}";

        // a=fmtp:108 level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f
        // Fmtp
        // {
        //  PayloadType = 108,
        //  Value = "level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f"
        // }
        // Dictionary
        //  {
        //      { "level-asymmetry-allowed", "1" },
        //      { "packetization-mode", "0" },
        //      { "profile-level-id", "42e01f" }
        //  }
        public static Dictionary<string, object> ToDictionary(this Fmtp fmtp)
        {
            Dictionary<string, object/*string*/> dictionary = new();

            var tokens = fmtp.Value.Split(';');
            foreach (var token in tokens)
            {
                var subTokens = token.Split('=');
                if (subTokens.Length == 1)
                    dictionary.Add(subTokens[0], null);
                else
                {
                    if (int.TryParse(subTokens[1], out int n))
                        dictionary.Add(subTokens[0], n);
                    else
                        dictionary.Add(subTokens[0], subTokens[1]);
                }
            }
            return dictionary;
        }


        public static RtcpFb ToRtcpFb(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(RtcpFb.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new RtcpFb
            {
                PayloadType = int.Parse(tokens[0]),
                Type = tokens[1],
                SubType = tokens.Length > 2 ? tokens[2] : null
            };
        }

        public static string ToText(this RtcpFb rtcpFb) =>
            $"{Sdp.AttributeIndicator}{RtcpFb.Label}" +
                $"{rtcpFb.PayloadType} {rtcpFb.Type} {rtcpFb.SubType ?? string.Empty} " +
                $"{Sdp.CRLF}";

        public static Extmap ToExtmap(this string str)
        {
            var tokens = str
                .Replace(Sdp.AttributeIndicator, string.Empty)
                .Replace(Extmap.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var subtokens = tokens[0].Split('/');
            return new Extmap
            {
                Value = int.Parse(subtokens[0]),
                Direction = subtokens.Length > 1 ? subtokens[1].EnumFromDisplayName<Direction>() : null,
                Uri = new Uri(tokens[1]),
                ExtensionAttributes = tokens.Length > 2 ? tokens[2] : null 
            };
        }

        public static string ToText(this Extmap extmap) =>
            $"{Sdp.AttributeIndicator}{Extmap.Label}" +
                 $"{extmap.Value}{(extmap.Direction.HasValue ? extmap.Direction.DisplayName() : string.Empty)} " +
                 $"{extmap.Uri} {extmap.ExtensionAttributes ?? string.Empty}" +
                 $"{Sdp.CRLF}";



        // Utility methods.

        //public static Rtpmap[] ToRtpmaps(this MediaDescription mediaDescription)
        //{
        //    var attributes = mediaDescription.AttributesOld
        //        .Where(a => a.StartsWith("rtpmap:"))
        //        .ToArray();

        //    List<Rtpmap> rtpmapAttributes = new();
        //    foreach (var a in attributes)
        //    {
        //        var tokens = a.Substring(7).Split(new[] { ' ', '/' }, 4);
        //        rtpmapAttributes.Add(new Rtpmap
        //        {
        //            PayloadType = int.Parse(tokens[0]),
        //            EncodingName = tokens[1],
        //            ClockRate = int.Parse(tokens[2]),
        //            Channels = tokens.Length == 4 ? int.Parse(tokens[3]) : null
        //        });
        //    }

        //    return rtpmapAttributes.ToArray();
        //}

        //public static Fmtp[] ToFmtps(this MediaDescription mediaDescription)
        //{
        //    var attributes = mediaDescription.AttributesOld
        //        .Where(a => a.StartsWith("fmtp:"))
        //        .ToArray();

        //    List<Fmtp> fmtpAttributes = new();
        //    foreach (var a in attributes)
        //    {
        //        var tokens = a.Substring(5).Split(new char[] { ' ' }, 2);
        //        fmtpAttributes.Add(new Fmtp
        //        {
        //            PayloadType = int.Parse(tokens[0]),
        //            Value = tokens[1],
        //        });
        //    }

        //    return fmtpAttributes.ToArray();
        //}

        //public static RtcpFb[] ToRtcpFbs(this MediaDescription mediaDescription)
        //{
        //    var attributes = mediaDescription.AttributesOld
        //        .Where(a => a.StartsWith("rtcp-fb:"))
        //        .ToArray();

        //    List<RtcpFb> rtcpFbAttributes = new();
        //    foreach (var a in attributes)
        //    {
        //        var tokens = a.Substring(8).Split(new char[] { ' ' }, 3);
        //        rtcpFbAttributes.Add(new RtcpFb
        //        {
        //            PayloadType = int.Parse(tokens[0]),
        //            Type = tokens[1],
        //            SubType = tokens.Length == 3 ? tokens[2] : null
        //        });
        //    }

        //    return rtcpFbAttributes.ToArray();
        //}

        //public static Extmap[] ToExtmaps(this MediaDescription mediaDescription)
        //{
        //    var attributes = mediaDescription.AttributesOld
        //        .Where(a => a.StartsWith("extmap:"))
        //        .ToArray();

        //    List<Extmap> extmapAttributes = new();
        //    foreach (var a in attributes)
        //    {
        //        // Direction is optional and attached to Value with '/'.
        //        var tokens = a.Substring(7).Split(new char[] { ' ' }, 3);
        //        var subtokens = tokens[0].Split(new char[] { '/' }, 2);
        //        extmapAttributes.Add(new Extmap
        //        {
        //            Value = int.Parse(subtokens[0]),
        //            Direction = subtokens.Length == 2 ? subtokens[1] : null,
        //            Uri = tokens[1],
        //            ExtensionAttributes = tokens.Length == 3 ? tokens[2] : null
        //        });
        //    }

        //    return extmapAttributes.ToArray();
        //}

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

        public static long ToSeconds(this string str)
        {
            // Converts strings ending with the following letters to seconds.
            //  <digits>d - days (86400 seconds)
            //  <digits>h - hours (3600 seconds)
            //  <digits>m - minutes (60 seconds)
            //  <digits>s - seconds 
            if (str.EndsWith("d"))
                return long.Parse(str.TrimEnd('d')) * 86400;
            else if (str.EndsWith("h"))
                return long.Parse(str.TrimEnd('h')) * 3600;
            else if (str.EndsWith("m"))
                return long.Parse(str.TrimEnd('h')) * 3600;
            else if (str.EndsWith("s"))
                return long.Parse(str.TrimEnd('h')) * 3600;
            else
                throw new NotSupportedException();
        }
    }
}
