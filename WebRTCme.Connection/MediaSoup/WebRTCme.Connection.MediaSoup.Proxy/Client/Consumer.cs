using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Consumer
    {
		bool _closed;
		bool _paused;

        public event EventHandler OnClose;
        public event EventHandler OnTransportClosed;
        public event EventHandler OnTrackEnded;

        public Consumer(string id, string localId, string producerId, IRTCRtpReceiver rtpReceiver, 
            IMediaStreamTrack track, RtpParameters rtpParameters, object appData)
        {
            Id = id;
            LocalId = localId;
            ProducerId = producerId;
            RtpReceiver = rtpReceiver;
            Track = track;
            RtpParameters = rtpParameters;
            AppData = appData;
            _paused = !track.Enabled;

            HandleTrack();
        }


        public string Id { get; }
        public string LocalId { get; }
        public string ProducerId { get; }
        public bool Closed => _closed;
        public MediaKind Kind => Track.Kind.ToMediaSoup();
        public IRTCRtpReceiver RtpReceiver { get; }
        public IMediaStreamTrack Track { get; }
        public RtpParameters RtpParameters { get; }
        public bool Paused => _paused;
        public object AppData { get; }


        public void Close()
        {
            if (_closed)
                return;

            Console.WriteLine("close()");

            _closed = true;

            DestroyTrack();

            OnClose?.Invoke(this, EventArgs.Empty);
        }

        public void TransportClosed()
        {
            if (_closed)
                return;

            Console.WriteLine("TransportClosed()");

            _closed = true;

            DestroyTrack();

            OnTransportClosed?.Invoke(this, EventArgs.Empty);
        }

        public async Task<IRTCStatsReport> GetStats()
        {
            if (_closed)
                throw new Exception("closed");
            
            return await RtpReceiver.GetStats();
        }

        public void Pause()
        {
            if (_closed)
                return;

            _paused = true;
            Track.Enabled = false;
        }

        public void Resume()
        {
            if (_closed)
                return;

            _paused = false;
            Track.Enabled = true;
        }

        void HandleTrack()
        {
            Track.OnEnded += TrackEndedEvent;
        }


        void DestroyTrack()
        {
            try
            {
                Track.OnEnded -= TrackEndedEvent;
                Track.Stop();
            }
            catch
            { }
        }
        
        void TrackEndedEvent(object sender, EventArgs e)
        {
            OnTrackEnded?.Invoke(this, EventArgs.Empty);
        }

    }
}
