using System;
using System.Collections.Generic;
using System.Text;
using WebRtc.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public IWindow Window => new Window();
    }
}
