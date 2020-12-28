using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class XirsysClient : ITurnServerClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public XirsysClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<RTCIceServer[]> GetIceServers()
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress =  new Uri(new Uri(_configuration["TurnServerUrl:Xirsys"]), 
                _configuration["TurnServerChannel:Xirsys"]);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", _configuration["TurnServerAuthorization:Xirsys"]);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.PutAsync(string.Empty, new StringContent(string.Empty))
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
             

            return null;
        }
    }
}