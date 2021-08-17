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

        public static Sdp ToSdp(this string str)
        {
            Sdp sdp = new();

            var tokens = str.Split(new string[] { Constants.CRLF }, StringSplitOptions.RemoveEmptyEntries);
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

                    // Value attributes.
                    else if (attr.StartsWith(Group.Label))
                        sdp.Attributes.Group = attr.ToGroup();
                    else if (attr.StartsWith(MsidSemantic.Label))
                        sdp.Attributes.MsidSemantic = attr.ToMsidSemantic();

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
                    
                    // Value attributes.
                    else if (attr.StartsWith(Group.Label))
                        md.Attributes.Group = attr.ToGroup();
                    else if (attr.StartsWith(MsidSemantic.Label))
                        md.Attributes.MsidSemantic = attr.ToMsidSemantic();
                    else if (attr.StartsWith(Mid.Label))
                        md.Attributes.Mid = attr.ToMid();
                    else if (attr.StartsWith(Msid.Label))
                        md.Attributes.Msid = attr.ToMsid();
                    else if (attr.StartsWith(Candidate.Label))
                        md.Attributes.Candidate = attr.ToCandidate();
                    else if (attr.StartsWith(IceUfrag.Label))
                        md.Attributes.IceUfrag = attr.ToIceUfrag();
                    else if (attr.StartsWith(IcePwd.Label))
                        md.Attributes.IcePwd = attr.ToIcePwd();
                    else if (attr.StartsWith(IceOptions.Label))
                        md.Attributes.IceOptions = attr.ToIceOptions();
                    else if (attr.StartsWith(Fingerprint.Label))
                        md.Attributes.Fingerprint = attr.ToFingerprint();
                    else if (attr.StartsWith(Fingerprint.Label))
                        md.Attributes.Ssrc = attr.ToSsrc();
                    else if (attr.StartsWith(Ssrc.Label))
                        md.Attributes.SsrcGroup = attr.ToSsrcGroup();
                    else if (attr.StartsWith(Rid.Label))
                        md.Attributes.Rid = attr.ToRid();
                    else if (attr.StartsWith(Rtpmap.Label))
                        md.Attributes.Rtpmap = attr.ToRtpmap();
                    else if (attr.StartsWith(Fmtp.Label))
                        md.Attributes.Fmtp = attr.ToFmtp();
                    else if (attr.StartsWith(RtcpFb.Label))
                        md.Attributes.RtcpFb = attr.ToRtcpFb();

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

        public static string ToText(this Sdp sdp)
        {
            StringBuilder sb = new();

            // Session fields.
            sb.Append(ToProtocolVersionText(sdp.ProtocolVersion));
            sb.Append(ToText(sdp.Origin));
            sb.Append(ToSessionNameText(sdp.SessionName));
            if (sdp.SessionInformation is not null)
                sb.Append(ToInformationText(sdp.SessionInformation));
            if (sdp.Uri is not null)
                sb.Append(ToText(sdp.Uri));
            if (sdp.EmailAddresses is not null)
                sb.Append(ToEmailAddressesText(sdp.EmailAddresses));
            if (sdp.PhoneNumbers is not null)
                sb.Append(ToPhoneNumbersText(sdp.PhoneNumbers));
            if (sdp.ConnectionData is not null)
                sb.Append(ToText(sdp.ConnectionData));
            if (sdp.Bandwidths is not null)
                sdp.Bandwidths.Select(b => sb.Append(ToText(b)));
            sdp.Timings.Select(t => sb.Append(ToText(t)));
            if (sdp.RepeatTimes is not null)
                sdp.RepeatTimes.Select(r => sb.Append(ToText(r)));
            if (sdp.TimeZones is not null)
                sb.Append(ToText(sdp.TimeZones));
            if (sdp.EncryptionKey is not null)
                sb.Append(ToText(sdp.EncryptionKey));
            
            // Session binary attributes.
            if (sdp.Attributes.ExtmapAllowMixed.HasValue)
                sb.Append($"{Sdp.AttributeIndicator}{Attributes.ExtmapAllowMixedLabel}");

            // Session value attributes.
            if (sdp.Attributes.Group is not null)
                sb.Append(ToText(sdp.Attributes.Group));
            if (sdp.Attributes.MsidSemantic is not null)
                sb.Append(ToText(sdp.Attributes.MsidSemantic));


            // Media description fields.
            foreach (var md in sdp.MediaDescriptions)
            {
                sb.Append(ToText(md));

                if (md.Information is not null)
                    sb.Append(ToInformationText(md.Information));
                if (md.ConnectionData is not null)
                    sb.Append(ToText(md.ConnectionData));
                if (md.Bandwidths is not null)
                    md.Bandwidths.Select(b => sb.Append(ToText(b)));
                if (md.EncryptionKey is not null)
                    sb.Append(ToText(md.EncryptionKey));

                // Media description binary attributes.
                if (md.Attributes.ExtmapAllowMixed.HasValue)
                    sb.Append($"{Sdp.AttributeIndicator}{Attributes.ExtmapAllowMixedLabel}");

                // Media description value attributes.
                if (md.Attributes.Group is not null)
                    sb.Append(ToText(md.Attributes.Group));
                if (md.Attributes.MsidSemantic is not null)
                    sb.Append(ToText(md.Attributes.MsidSemantic));
                if (md.Attributes.Mid is not null)
                    sb.Append(ToText(md.Attributes.Mid));
                if (md.Attributes.Msid is not null)
                    sb.Append(ToText(md.Attributes.Msid));
                if (md.Attributes.Candidate is not null)
                    sb.Append(ToText(md.Attributes.Candidate));
                if (md.Attributes.IceUfrag is not null)
                    sb.Append(ToText(md.Attributes.IceUfrag));
                if (md.Attributes.IcePwd is not null)
                    sb.Append(ToText(md.Attributes.IcePwd));
                if (md.Attributes.IceOptions is not null)
                    sb.Append(ToText(md.Attributes.IceOptions));
                if (md.Attributes.Fingerprint is not null)
                    sb.Append(ToText(md.Attributes.Fingerprint));
                if (md.Attributes.Ssrc is not null)
                    sb.Append(ToText(md.Attributes.Ssrc));
                if (md.Attributes.SsrcGroup is not null)
                    sb.Append(ToText(md.Attributes.SsrcGroup));
                if (md.Attributes.Rid is not null)
                    sb.Append(ToText(md.Attributes.Rid));
                if (md.Attributes.Rtpmap is not null)
                    sb.Append(ToText(md.Attributes.Rtpmap));
                if (md.Attributes.Fmtp is not null)
                    sb.Append(ToText(md.Attributes.Fmtp));
                if (md.Attributes.RtcpFb is not null)
                    sb.Append(ToText(md.Attributes.RtcpFb));
            }

            return sb.ToString();
        }

        public static int ToProtocolVersion(this string str)
        {
            var token = str
                .Replace(Sdp.ProtocolVersionIndicator, string.Empty)
                .Replace(Constants.CRLF, string.Empty);
            return int.Parse(token);
        }

        public static string ToProtocolVersionText(this int version, bool withCRLF = true) =>
            $"{Sdp.ProtocolVersionIndicator}{version}{(withCRLF ? Constants.CRLF : string.Empty)}";

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

        public static string ToText(this Origin origin, bool withCRLF = true) =>
            $"{Sdp.OriginIndicator}{origin.UserName} {origin.SessionId} {origin.SessionVersion} " +
                $"{origin.NetType.DisplayName()} {origin.AddrType.DisplayName()} {origin.UnicastAddress}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static string ToSessionName(this string str)
        {
            var token = str
                .Replace(Sdp.SessionNameIndicator, string.Empty)
                .Replace(Constants.CRLF, string.Empty);
            return token;
        }

        public static string ToSessionNameText(this string name, bool withCRLF = true) =>
            $"{Sdp.SessionNameIndicator}{name}{(withCRLF ? Constants.CRLF : string.Empty)}";


        public static string ToInformation(this string str)
        {
            var token = str
                .Replace(Sdp.InformationIndicator, string.Empty)
                .Replace(Constants.CRLF, string.Empty);
            return token;
        }

        public static string ToInformationText(this string info, bool withCRLF = true) =>
            $"{Sdp.InformationIndicator}{info}{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static Uri ToUri(this string str)
        {
            var token = str
                .Replace(Sdp.UriIndicator, string.Empty)
                .Replace(Constants.CRLF, string.Empty);
            return new Uri(token);
        }

        public static string ToText(this Uri uri, bool withCRLF = true) =>
            $"{Sdp.UriIndicator}{uri}{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static List<string> ToEmailAddresses(this string str)
        {
            var token = str
                .Replace(Sdp.EmailAddressIndicator, string.Empty)
                .Replace(Constants.CRLF, string.Empty);

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

        public static string ToEmailAddressesText(this IList<string> emails, bool withCRLF = true) =>
            $"{Sdp.EmailAddressIndicator}{string.Join(" ", emails)}{(withCRLF? Constants.CRLF : string.Empty)}";

        public static List<string> ToPhoneNumbers(this string str)
        {
            var token = str
                 .Replace(Sdp.PhoneNumberIndicator, string.Empty)
                 .Replace(Constants.CRLF, string.Empty);

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

        public static string ToPhoneNumbersText(this IList<string> phones, bool withCRLF = true) =>
            $"{Sdp.PhoneNumberIndicator}{string.Join(" ", phones)}{(withCRLF ? Constants.CRLF : string.Empty)}";

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

        public static string ToText(this ConnectionData connectionData, bool withCRLF = true) =>
            $"{Sdp.ConnectionDataIndicator}{connectionData.NetType.DisplayName()} " +
                $"{connectionData.AddrType.DisplayName()} {connectionData.ConnectionAddress}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

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

        public static string ToText(this Bandwidth bandwidth, bool withCRLF = true) =>
            $"{Sdp.BandwidthIndicator}{bandwidth.Type} {bandwidth.Value}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

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

        public static string ToText(this Timing timing, bool withCRLF = true) =>
            $"{Sdp.TimingIndicator}{(timing.StartTime - new DateTime(1900, 1, 1)).TotalSeconds} " +
                $"{(timing.StopTime - new DateTime(1900, 1, 1)).TotalSeconds}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

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

        public static string ToText(this RepeatTime repeatTime, bool withCRLF = true) =>
            $"{Sdp.RepeatTimeIndicator}{repeatTime.RepeatInterval.TotalSeconds} " +
                $"{repeatTime.ActiveDuration.TotalSeconds} " +
                $"{string.Join(" ", repeatTime.OffsetsFromStartTime.Select(o => o.TotalSeconds).ToArray())}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

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

        public static string ToText(this IList<TimeZone> timeZones, bool withCRLF = true) =>
            $"{Sdp.TimeZoneIndicator}" +
                $"{string.Join(" ", timeZones.Select(z => (z.AdjustmentTime - new DateTime(1900, 1, 1)).TotalSeconds.ToString() + " " + z.Offset.TotalSeconds.ToString()))}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

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

        public static string ToText(this EncryptionKey encryptionKey, bool withCRLF = true) =>
            $"{Sdp.EncryptionKeyIndicator}{encryptionKey.Method.DisplayName()}:{encryptionKey.Value}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";


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

        public static string ToText(this MediaDescription mediaDescription, bool withCRLF = true) =>
            $"{Sdp.MediaDescriptionIndicator}" +
                $"{mediaDescription.Media.DisplayName()} {mediaDescription.Port} {mediaDescription.Proto} " +
                $"{string.Join(" ", mediaDescription.Fmts.ToArray())}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        
        // Attributes.

        public static Group ToGroup(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Group.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Group
            {
                Semantics = tokens[0].EnumFromDisplayName<GroupSemantics>(),
                SemanticsExtensions = tokens.Skip(1).ToArray()
            };
        }

        public static string ToText(this Group group, bool withAttributeCharacter = true, bool withCRLF = true) =>
            $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Group.Label}{group.Semantics} " +
                $"{string.Join(" ", group.SemanticsExtensions)}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static MsidSemantic ToMsidSemantic(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                .Replace(MsidSemantic.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new MsidSemantic
            {
                Token = tokens[0],
                IdList = tokens.Length > 1 ? tokens.Skip(1).ToArray() : null
            };
        }
        
        public static string ToText(this MsidSemantic msidSemantic, bool withAttributeCharacter = true, 
            bool withCRLF = true) =>
            $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{MsidSemantic.Label}" +
                $"{msidSemantic.Token} {string.Join(" ", msidSemantic.IdList)}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";



        public static Mid ToMid(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Mid.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Mid
            {
                Id = tokens[0]
            };
        }

        public static string ToText(this Mid mid, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
            $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Mid.Label}" +
                $"{mid.Id}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static Msid ToMsid(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Msid.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Msid
            {
                Id = tokens[0],
                AppData = tokens[1]
            };
        }
        
        public static string ToText(this Msid msid, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
            $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Msid.Label}" +
                $"{msid.Id} {msid.AppData}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static Candidate ToCandidate(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                .Replace(Candidate.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Candidate
            {
                Foundation = tokens[0],
                ComponentId = int.Parse(tokens[1]),
                Transport = tokens[2].EnumFromDisplayName<CandidateTransport>(),
                Priority = int.Parse(tokens[3]),
                ConnectionAddress = tokens[4],
                Port = int.Parse(tokens[5]),
                Type = tokens[7].EnumFromDisplayName<CandidateType>(), 
                RelAddr = tokens[9],
                RelPort = int.Parse(tokens[11])
            };
        }

        public static string ToText(this Candidate candidate, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
            $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Candidate.Label}" +
                $"{candidate.Foundation} {candidate.ComponentId} {candidate.Transport.DisplayName()} " +
                $"{candidate.Priority} {candidate.ConnectionAddress} {candidate.Port} {Candidate.Typ} " +
                $"{candidate.Type.DisplayName()} {Candidate.Raddr} {candidate.RelAddr} " +
                $"{Candidate.Rport} {candidate.RelPort}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static IceUfrag ToIceUfrag(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(IceUfrag.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new IceUfrag
            {
                Ufrag = tokens[0]
            };
        }

        public static string ToText(this IceUfrag iceUfrag, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
            $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{IceUfrag.Label}" +
                $"{iceUfrag.Ufrag}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static IcePwd ToIcePwd(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(IcePwd.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new IcePwd
            {
                Password = tokens[0]
            };
        }

        public static string ToText(this IcePwd icePwd, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
            $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{IcePwd.Label}" +
                $"{icePwd.Password}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static IceOptions ToIceOptions(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(IceOptions.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new IceOptions
            {
                Tags = tokens
            };
        }

        public static string ToText(this IceOptions iceOptions, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
            $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{IceOptions.Label}" +
                $"{string.Join(" ", iceOptions.Tags)}" +
                $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static Fingerprint ToFingerprint(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(Fingerprint.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new Fingerprint
            {
                HashFunction = tokens[0].EnumFromDisplayName<HashFunction>(), 
                HashValue = HexadecimalStringToByteArray(tokens[1].Replace(":", string.Empty))
            };
        }

        public static string ToText(this Fingerprint fingerprint, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
                $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Fingerprint.Label}" +
                    $"{fingerprint.HashFunction.DisplayName()} " +
                    $"{BitConverter.ToString(fingerprint.HashValue).Replace("-", ":")}" +
                    $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static Ssrc ToSsrc(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
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

        public static string ToText(this Ssrc ssrc, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
                $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Ssrc.Label}" +
                    $"{ssrc.Id} {ssrc.Attribute} {ssrc.Value ?? string.Empty}" +
                    $"{(withCRLF ? Constants.CRLF : string.Empty)}";
        

        public static SsrcGroup ToSsrcGroup(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                .Replace(SsrcGroup.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new SsrcGroup
            {
                Semantics = tokens[0],
                SsrcIds = tokens.Skip(1).ToArray()
            };
        }

        public static string ToText(this SsrcGroup ssrcGroup, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
                $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{SsrcGroup.Label}" +
                    $"{ssrcGroup.Semantics} {string.Join(" ", ssrcGroup.SsrcIds)}" +
                    $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static Rid ToRid(this string str)
        {
            var tokens = str
                .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                .Replace(Rid.Label, string.Empty)
                .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var subTokens = tokens[2].Split(';');
            var fmtList = subTokens.Where(st => st.StartsWith("pt=")).ToArray();
            var restrictions = subTokens.Skip(fmtList.Length).Take(subTokens.Length - fmtList.Length).ToArray();
            return new Rid
            {
                Id = tokens[0],
                Direction = tokens[1].EnumFromDisplayName<RidDirection>(), 
                FmtList = fmtList,
                Restrictions = restrictions
            };
        }

        public static string ToText(this Rid rid, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
                $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Rid.Label}" +
                    $"{rid.Id} {rid.Direction.DisplayName()} " +
                    $"{string.Join(";", rid.FmtList.Select(f => "pt=" + f)).ToArray()}" +
                    $"{string.Join(";", rid.Restrictions)}" +
                    $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static Rtpmap ToRtpmap(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
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

        public static string ToText(this Rtpmap rtpmap, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
                $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Rtpmap.Label}" +
                    $"{rtpmap.PayloadType} {rtpmap.EncodingName}/{rtpmap.ClockRate}" +
                    $"{(rtpmap.Channels.HasValue ? "/" + rtpmap.Channels : string.Empty)}" +
                    $"{(withCRLF ? Constants.CRLF : string.Empty)}";

        public static Fmtp ToFmtp(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
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

        public static string ToText(this Fmtp fmtp, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
                $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{Fmtp.Label}" +
                    $"{fmtp.PayloadType} {fmtp.Value}" +
                    $"{(withCRLF ? Constants.CRLF : string.Empty)}";


        public static RtcpFb ToRtcpFb(this string str)
        {
            var tokens = str
                 .Replace(SdpSerializer.AttributeCharacter, string.Empty)
                 .Replace(RtcpFb.Label, string.Empty)
                 .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new RtcpFb
            {
                PayloadType = int.Parse(tokens[0]),
                Type = tokens[1],
                SubType = tokens.Length > 2 ? tokens[2] : null
            };
        }

        public static string ToText(this RtcpFb rtcpFb, bool withAttributeCharacter = true,
            bool withCRLF = true) =>
                $"{(withAttributeCharacter ? Sdp.AttributeIndicator : string.Empty)}{RtcpFb.Label}" +
                    $"{rtcpFb.PayloadType} {rtcpFb.Type} {rtcpFb.SubType ?? string.Empty} " +
                    $"{(withCRLF ? Constants.CRLF : string.Empty)}";


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
                if (int.TryParse(subTokens[1], out int n))
                    dictionary.Add(subTokens[0], n);
                else
                dictionary.Add(subTokens[0], subTokens[1]);
            }
            return dictionary;
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
