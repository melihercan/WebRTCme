using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Connection.Signaling.Server.TurnServerProxies.Enums;
using WebRTCme.Connection.Signaling.Server.TurnServerProxies.Models;

namespace WebRTCme.Connection.Signaling.Server.TurnServerProxies
{
    public class XirsysProxy : ITurnServerProxy
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public XirsysProxy(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<RTCIceServer[]> GetIceServersAsync()
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

            var json = await response.Content.ReadAsStringAsync();
            var xirsysTurnResponse = JsonSerializer.Deserialize<XirsysTurnResponse>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });
            if (xirsysTurnResponse.S == XirsysTurnStatusResponse.Error)
                throw new Exception("Xirsys turn server reported error");

            var iceServers = xirsysTurnResponse.V.IceServers
                .Select(xirsysIceServer => new RTCIceServer 
                { 
                    Credential = xirsysIceServer.Credential,
                    CredentialType = RTCIceCredentialType.Password,
                    Urls = new string[] { xirsysIceServer.Url },
                    Username = xirsysIceServer.Username
                }).ToArray();
            return iceServers;
        }
    }
}
