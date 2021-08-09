using System;
using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class Sdp
    {
        /// <summary>
        /// 'v=0'
        /// </summary>
        public int ProtocolVersion { get; set; }

        /// <summary>
        /// o=<username> <sess-id> <sess-version> <nettype> <addrtype> <unicast-address>
        /// </summary>
        public Origin Origin { get; set; }

        /// <summary>
        /// s=<session name>
        /// </summary>
        public string SessionName { get; set; }
        public const string DefaultSessionName = "-";

        /// <summary>
        /// i=<session description>
        /// Optional.
        /// </summary>
        public string SessionInformation { get; set; }

        /// <summary>
        /// u=<uri>
        /// Optional.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// e=<email-address> 
        /// Optional.
        /// More than one can be specified.
        /// </summary>
        public IList<string> EmailAddresses { get; set; }

        /// <summary>
        /// p=<phone-number>
        /// Optional.
        /// More than one can be specified.
        /// </summary>
        public IList<string> PhoneNumbers { get; set; }

        /// <summary>
        /// c=<nettype> <addrtype> <connection-address>
        /// Either here or in media descriptions, so it is optional here.
        /// </summary>
        public ConnectionData ConnectionData { get; set; }

        /// <summary>
        /// b=<bwtype>:<bandwidth>
        /// Either here or in media descriptions, so it is optional here.
        /// </summary>
        public Bandwidth Bandwidth { get; set; }

        /// <summary>
        /// t=<start-time> <stop-time>
        /// Can have multiple entries.
        /// </summary>
        public IList<Timing> Timings { get; set; }

        /// <summary>
        /// r=<repeat interval> <active duration> <offsets from start-time>
        /// Optional.
        /// Can have multiple entries.
        /// </summary>
        public IList<RepeatTime> RepeatTimes { get; set; }

        /// <summary>
        /// z=<adjustment time> <offset> <adjustment time> <offset> ....
        /// Optional.
        /// </summary>
        public IList<TimeZone> TimeZones { get; set; }

        /// <summary>
        /// k=<method>
        /// k=<method>:<encryption key>
        /// Either here or in media descriptions, so it is optional here.
        /// Not recommended, new work is in progress.
        /// </summary>
        public EncriptionKey EncriptionKey { get; set; }




    }
}
