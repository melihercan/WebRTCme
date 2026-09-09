using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IWebRtcIncomingFileStreamFactory
    {
        Task<Stream> CreateAsync(string peerUserName, DataParameters dataParameters, 
            Action<string/*peerUserName*/, Guid/*fileGuid*/> onCompleted);
    }
}
