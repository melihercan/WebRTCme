using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStream : IAsyncDisposable
    {




        Task SetMediaSourceAsync(object media);
    }
}
