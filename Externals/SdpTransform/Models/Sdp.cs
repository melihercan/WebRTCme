using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Sdp
    {
        /// <summary>
        /// v=0
        /// </summary>
        public int ProtocolVersion { get; set; }
        public const string ProtocolVersionIndicator = "v=";

        /// <summary>
        /// o=<username> <sess-id> <sess-version> <nettype> <addrtype> <unicast-address>
        /// </summary>
        public Origin Origin { get; set; }
        public const string OriginIndicator = "o=";

        /// <summary>
        /// s=<session name>
        /// </summary>
        public string SessionName { get; set; }
        public const string SessionNameIndicator = "s=";
        public const string DefaultSessionName = "-";

        /// <summary>
        /// i=<session description>
        /// Optional.
        /// </summary>
        public string SessionInformation { get; set; }
        public const string InformationIndicator = "i=";

        /// <summary>
        /// u=<uri>
        /// Optional.
        /// </summary>
        public Uri Uri { get; set; }
        public const string UriIndicator = "u=";

        /// <summary>
        /// e=<email-address> 
        /// Optional.
        /// More than one can be specified.
        /// </summary>
        public IList<string> EmailAddresses { get; set; }
        public const string EmailAddressIndicator = "e=";

        /// <summary>
        /// p=<phone-number>
        /// Optional.
        /// More than one can be specified.
        /// </summary>
        public IList<string> PhoneNumbers { get; set; }
        public const string PhoneNumberIndicator = "p=";

        /// <summary>
        /// c=<nettype> <addrtype> <connection-address>
        /// Either here or in each media descriptions, so it is optional here.
        /// </summary>
        public ConnectionData ConnectionData { get; set; }
        public const string ConnectionDataIndicator = "c=";

        /// <summary>
        /// b=<bwtype>:<bandwidth>
        /// Either here or in media descriptions, so it is optional here.
        /// Can have multiple entries.
        /// </summary>
        public IList<Bandwidth> Bandwidths { get; set; }
        public const string BandwidthIndicator = "b=";


        /// <summary>
        /// t=<start-time> <stop-time>
        /// Time values in seconds since 1900.
        /// Can have multiple entries.
        /// </summary>
        public IList<Timing> Timings { get; set; }
        public const string TimingIndicator = "t=";

        /// <summary>
        /// r=<repeat interval> <active duration> <offsets from start-time>
        /// Optional.
        /// Can have multiple entries.
        /// </summary>
        public IList<RepeatTime> RepeatTimes { get; set; }
        public const string RepeatTimeIndicator = "r=";

        /// <summary>
        /// z=<adjustment time> <offset> <adjustment time> <offset> ....
        /// Optional.
        /// </summary>
        public IList<TimeZone> TimeZones { get; set; }
        public const string TimeZoneIndicator = "z=";

        /// <summary>
        /// k=<method>
        /// k=<method>:<encryption key>
        /// Either here or in media descriptions, so it is optional here.
        /// Not recommended, new work is in progress.
        /// </summary>
        public EncryptionKey EncryptionKey { get; set; }
        public const string EncryptionKeyIndicator = "k=";

        // Attributes.
        public Attributes Attributes { get; set; }
        public const string AttributeIndicator = "a=";


        // MediaDescriptions.
        public IList<MediaDescription> MediaDescriptions { get; set; }
        public const string MediaDescriptionIndicator = "m=";


        // Constants.
        public const string CRLF = "\r\n";

    }
}
