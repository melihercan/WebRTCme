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

#if false
            var x1 = new XirsysTurnResponse
            {
                S = XirsysTurnStatusResponse.Ok,
                V = new XirsysTurnValueResponse
                {
                    IceServers = new XirsysTurnIceServerResponse[]
                    {
                        new XirsysTurnIceServerResponse
                        {
                            Url = "stun:eu-turn5.xirsys.com"
                        },
                        new XirsysTurnIceServerResponse
                        {
                            Username = "0QhBDZyu8dIqP8oEY10ZxmZppbmxfYZWBXxoMtrHqTV8mrsZIZ4iMfzfkHM_CtCvAAAAAF_rA5VtZWxpaGVyY2FuNDg=",
                            Url = "turn:eu-turn5.xirsys.com:80?transport=udp",
                            Credential = "de142dd0-49bf-11eb-9ec6-0242ac140004"
                        },
                        new XirsysTurnIceServerResponse
                        {
                            Username = "0QhBDZyu8dIqP8oEY10ZxmZppbmxfYZWBXxoMtrHqTV8mrsZIZ4iMfzfkHM_CtCvAAAAAF_rA5VtZWxpaGVyY2FuNDg=",
                            Url = "turn:eu-turn5.xirsys.com:3478?transport=udp",
                            Credential = "de142dd0-49bf-11eb-9ec6-0242ac140004"
                        }
                    }
                }
            };
            var o = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            var x2 = JsonSerializer.Serialize(x1, o);
            var x3 = JsonSerializer.Deserialize<XirsysTurnResponse>(x2, o);
#endif


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
                .Select(xirsisIceServer => new RTCIceServer 
                { 
                    Credential = xirsisIceServer.Credential,
                    CredentialType = RTCIceCredentialType.Password,
                    Urls = new string[] { xirsisIceServer.Url },
                    Username = xirsisIceServer.Username
                }).ToArray();
            return iceServers;
        }
    }
}
