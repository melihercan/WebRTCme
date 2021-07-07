using System.Collections.Generic;
using System.Text;

namespace SDPLib
{
    public class MediaDescription
    {
        public string Media { get; set; }

        public string Port { get; set; }

        public string Proto { get; set; }

        public IList<string> Fmts { get; set; }

        public byte[] Title { get; set; }

        public string TitleString(Encoding encoding) => encoding.GetString(Title);

        public ConnectionData ConnectionInfo { get; set; }

        public IList<Bandwidth> Bandwiths { get; set; }

        public EncriptionKey EncriptionKey { get; set; }

        public IList<string> Attributes { get; set; }
    }
}
