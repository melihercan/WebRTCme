using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Consumer
    {
		readonly string _id;
		readonly string _localId;
		readonly string _producerId;
		readonly IRTCRtpReceiver _rtpReceiver;
		readonly IMediaStreamTrack _track;
		readonly RtpParameters _rtpParameters;
		readonly object _appData;
		bool _closed;
		bool _paused;

        public event EventHandler OnClose;
        public event EventHandler OnTransportClosed;
        public event EventHandler OnTrackEnded;

        public Consumer(string id, string localId, string producerId, IRTCRtpReceiver rtpReceiver, 
            IMediaStreamTrack track, RtpParameters rtpParameters, object appData)
        {
            _id = id;
            _localId = localId;
            _producerId = producerId;
            _rtpReceiver = rtpReceiver;
            _track = track;
            _rtpParameters = rtpParameters;
            _appData = appData;
            _paused = !track.Enabled;

            HandleTrack();
        }


        public string Id => _id;
        public string LocalId => _localId;
        public string ProducerId => _producerId;
        public bool Closed => _closed;
        public MediaKind Kind => _track.Kind.ToMediaSoup();
        public IRTCRtpReceiver RtpReceiver => _rtpReceiver;
        public IMediaStreamTrack Track => _track;
        public RtpParameters RtpParameters => _rtpParameters;
        public bool Paused => _paused;
        public object AppData => _appData;


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
            
            return await _rtpReceiver.GetStats();
        }

        public void Pause()
        {
            if (_closed)
                return;

            _paused = true;
            _track.Enabled = false;
        }

        public void Resume()
        {
            if (_closed)
                return;

            _paused = false;
            _track.Enabled = true;
        }

        void HandleTrack()
        {
            _track.OnEnded += TrackEndedEvent;
        }


        void DestroyTrack()
        {
            try
            {
                _track.OnEnded -= TrackEndedEvent;
                _track.Stop();
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
