using System.Collections.Generic;
using System.Text;

namespace Utilme.SdpTransform
{
    public class MediaDescription
    {
        public MediaType Media { get; set; }

        public int Port { get; set; }

        public string Proto { get; set; }

        public IList<string> Fmts { get; set; }

        // Session overrides.

        /// <summary>
        /// i=<session description>
        /// Optional.
        /// </summary>
        public string Information { get; set; }

        /// <summary>
        /// c=<nettype> <addrtype> <connection-address>
        /// Either here or in media descriptions, so it is optional here.
        /// </summary>
        public ConnectionData ConnectionData { get; set; }

        /// <summary>
        /// b=<bwtype>:<bandwidth>
        /// Optional.
        /// </summary>
        public IList<Bandwidth> Bandwidths { get; set; }

        /// <summary>
        /// k=<method>
        /// k=<method>:<encryption key>
        /// Optional.
        /// Not recommended, new work is in progress.
        /// </summary>
        public EncryptionKey EncryptionKey { get; set; }



        // Attributes.
        public Attributes Attributes { get; set; }



        ////public byte[] Title { get; set; }

        /////public string TitleString(Encoding encoding) => encoding.GetString(Title);

        ////public ConnectionData ConnectionData { get; set; }

        ////public IList<Bandwidth> Bandwidths { get; set; }

        //public EncriptionKey EncriptionKey { get; set; }

        //public IList<string> AttributesOld { get; set; }
    }
}
