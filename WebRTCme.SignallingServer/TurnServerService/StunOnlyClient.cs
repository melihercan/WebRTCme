using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class StunOnlyClient : ITurnServerClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public StunOnlyClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public Task<RTCIceServer[]> GetIceServersAsync()
        {
            // Hard coded STUN servers. 
            return Task.FromResult(new RTCIceServer[] 
            { 
                new RTCIceServer
                {
                    Urls = new string[] 
                    {
                        "stun:stun.stunprotocol.org:3478",
                    },
                },
                new RTCIceServer
                {
                    Urls = new string[]
                    {
                        "stun:stun.l.google.com:19302"
                    },
                }
            });
        }
    }
}
