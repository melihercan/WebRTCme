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
        /// </summary>
        public string SessionInformation { get; set; }

        /// <summary>
        /// u=<uri>
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// e=<email-address> 
        /// More than one can be specified.
        /// </summary>
        public IList<string> EmailAddresses { get; set; }

        /// <summary>
        /// p=<phone-number>
        /// More than one can be specified.
        /// </summary>
        public IList<string> PhoneNumbers { get; set; }

        /// <summary>
        /// c=<nettype> <addrtype> <connection-address>
        /// </summary>
        public ConnectionData ConnectionData { get; set; }

    }
}
