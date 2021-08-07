using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Consumer
    {
        public event EventHandler OnClose;
        public event EventHandler OnTransportClosed;
        public event EventHandler OnTrackEnded;
        public event EventHandlerAsync<EventArgs, IRTCStatsReport> OnGetStatsAsync;


        public Consumer(string id, string localId, string producerId, IRTCRtpReceiver rtpReceiver,
            IMediaStreamTrack track, RtpParameters rtpParameters, Dictionary<string, object> appData)
        {
            Id = id;
            LocalId = localId;
            ProducerId = producerId;
            RtpReceiver = rtpReceiver;
            Track = track;
            RtpParameters = rtpParameters;
            AppData = appData;
            Paused = !track.Enabled;
            Closed = false;

            HandleTrack();
        }


        public string Id { get; }
        public string LocalId { get; }
        public string ProducerId { get; }
        public bool Closed { get; private set; }
        public MediaKind Kind => Track.Kind.ToMediaSoup();
        public IRTCRtpReceiver RtpReceiver { get; }
        public IMediaStreamTrack Track { get; }
        public RtpParameters RtpParameters { get; }
        public bool Paused { get; private set;}
        public Dictionary<string, object> AppData { get; }


        public void Close()
        {
            if (Closed)
                return;

            Console.WriteLine("close()");

            Closed = true;

            DestroyTrack();

            OnClose?.Invoke(this, EventArgs.Empty);
        }

        public void TransportClosed()
        {
            if (Closed)
                return;

            Console.WriteLine("TransportClosed()");

            Closed = true;

            DestroyTrack();

            OnTransportClosed?.Invoke(this, EventArgs.Empty);
        }

        public async Task<IRTCStatsReport> GetStats()
        {
            if (Closed)
                throw new Exception("closed");

            return await OnGetStatsAsync?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            if (Closed)
                return;

            Paused = true;
            Track.Enabled = false;
        }

        public void Resume()
        {
            if (Closed)
                return;

            Paused = false;
            Track.Enabled = true;
        }

        void HandleTrack()
        {
            Track.OnEnded += Consumer_TrackEnded;
        }


        void DestroyTrack()
        {
            try
            {
                Track.OnEnded -= Consumer_TrackEnded;
                Track.Stop();
            }
            catch
            { }
        }
        
        void Consumer_TrackEnded(object sender, EventArgs e)
        {
            OnTrackEnded?.Invoke(this, EventArgs.Empty);
        }

    }
}
