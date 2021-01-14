using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRtcMeMiddleware
{
    public class RoomContext
    {
        public RoomState RoomState { get; set; }
        public RoomRequestParameters RoomRequestParameters { get; set; }

        public TaskCompletionSource<IMediaStream> ConnectTcs { get; set; } = new();

        public TaskCompletionSource<Unit> DisconnectTcs { get; set; } = new();

        public static TaskCompletionSource<RoomResponseParameters> RoomStarted { get; set; } = new();
        public static TaskCompletionSource<RoomResponseParameters> RoomStopped { get; set; } = new();
        public static TaskCompletionSource<RoomResponseParameters> PeerJoined { get; set; } = new();
        public static TaskCompletionSource<RoomResponseParameters> PeerLeft { get; set; } = new();
        public static TaskCompletionSource<RoomResponseParameters> PeerSdpOffered { get; set; } = new();
        public static TaskCompletionSource<RoomResponseParameters> PeerSdpAnswered { get; set; } = new();

    }
}
