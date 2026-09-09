using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.ClientWebSockets
{
    public interface IClientWebSocketOptions
    {
        public bool IgnoreServerCertificateErrors { get; set; }
        public void AddSubProtocol(string subProtocol);
        public void SetRequestHeader(string headerName, string headerValue);
    }
}
