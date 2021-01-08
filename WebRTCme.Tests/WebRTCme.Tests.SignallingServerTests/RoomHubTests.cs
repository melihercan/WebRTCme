using System;
using System.Threading.Tasks;
using WebRTCme.SignallingServerClient;
using Xunit;

namespace WebRTCme.Tests.SignallingServerTests
{
    public class RoomHubTests
    {
        public RoomHubTests()
        {
            Console.WriteLine("These tests require live SignallingServer, make sure that server is running...");
        }

        [Fact]
        public async Task EchoToCallerTest()
        {
            var signallingServerClient = SignallingServerClientFactory.Create("https://192.168.1.48:5051");
            await signallingServerClient.InitializeAsync();

            var result = await signallingServerClient.ExecuteEchoToCaller("Echo me");

        }

    }
}
