using Blazorme;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware.Helpers;

namespace WebRTCme.Middleware.Services
{
    class WebRtcIncomingFileStreamFactory : IWebRtcIncomingFileStreamFactory
    {
        private readonly IStreamSaver _streamSaver;

        public WebRtcIncomingFileStreamFactory(IStreamSaver streamSaver)
        {
            _streamSaver = streamSaver;
        }

        public async Task<Stream> CreateAsync(string peerUserName, File file, DataParameters dataParameters, Action<string, Guid> onCompleted)
        {
            WebRtcIncomingFileStream webRtcIncomingFileStream = new(_streamSaver, peerUserName, file, dataParameters, 
                onCompleted);
            await webRtcIncomingFileStream.CreateAsync();
            return webRtcIncomingFileStream;
        }
    }
}
