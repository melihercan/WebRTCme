using System;
using System.Collections.Generic;
using System.Text;

namespace SDPLib
{
    public class SDP
    {
        /// <summary>
        /// The "v=" field gives the version of the Session Description Protocol.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The "o=" field gives the originator of the session (her username and
        /// the address of the user's host) plus a session identifier and version
        /// number
        /// </summary>
        public Origin Origin { get; set; }

        /// <summary>
        /// The "s=" field is the textual session name.
        /// </summary>
        public byte[] SessionName { get; set; }

        /// <summary>
        /// The "s=" field is the textual session name.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string SessionNameString(Encoding encoding) => encoding.GetString(SessionName);

        /// <summary>
        /// The "i=" field provides textual information about the session
        /// </summary>
        public byte[] SessionInformation { get; set; }

        /// <summary>
        /// The "i=" field provides textual information about the session
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string SessionInformationString(Encoding encoding) => encoding.GetString(SessionInformation);

        public ConnectionData ConnectionData { get; set; }

        public IList<TimingInfo> Timings { get; set; }

        public Uri Uri { get; set; }

        public IList<string> EmailNumbers { get; set; }

        public IList<string> PhoneNumbers { get; set; }

        public IList<Bandwidth> BandWiths { get; set; }

        public IList<RepeatTime> SDPRepeatTimes { get; set; }

        public IList<SDPTimezoneInfo> TimeZones { get; set; }

        public EncriptionKey EncriptionKey { get; set; }

        public IList<string> Attributes { get; set; }

        public IList<MediaDescription> MediaDescriptions { get; set; }
    }
}
