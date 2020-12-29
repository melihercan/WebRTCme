using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            httpClient.BaseAddress = new Uri(_configuration["TurnServerBaseUrl:Xirsys"] + "/" +
                _configuration["TurnServerChannel:Xirsys"]);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", 
                Convert.ToBase64String(
                    Encoding .GetEncoding(28591).GetBytes(_configuration["TurnServerAuthorization:Xirsys"])));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.PutAsync(string.Empty, new StringContent(string.Empty));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();


            return null;
        }
    }
}
