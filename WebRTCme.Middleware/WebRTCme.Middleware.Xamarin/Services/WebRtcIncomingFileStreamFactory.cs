using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public class WebRtcIncomingFileStreamFactory : IWebRtcIncomingFileStreamFactory
    {
        public Task<Stream> CreateAsync(string peerUserName, DataParameters dataParameters, Action<string, Guid> onCompleted)
        {
            throw new NotImplementedException();
        }
    }
}
