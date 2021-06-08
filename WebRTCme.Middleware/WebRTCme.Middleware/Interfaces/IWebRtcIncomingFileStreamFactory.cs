using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebRTCme.Middleware
{
    public interface IWebRtcIncomingFileStreamFactory
    {
        Stream Create(string peerUserName, File file, DataParameters dataParameters, Action cbCompleted);
    }
}
