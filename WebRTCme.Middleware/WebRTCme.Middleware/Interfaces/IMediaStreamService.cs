using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Middleware
{
    public interface IMediaStreamService
    {
        Task<IMediaStream> GetCameraStreamAsync(string source);
    }
}
