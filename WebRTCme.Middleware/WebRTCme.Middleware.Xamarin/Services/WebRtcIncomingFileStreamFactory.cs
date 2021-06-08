using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware
{
    public class WebRtcIncomingFileStreamFactory : IWebRtcIncomingFileStreamFactory
    {
        public Stream Create(string peerUserName, File file, DataParameters dataParameters, Action cbCompleted)
        {
            throw new NotImplementedException();
        }
    }
}
