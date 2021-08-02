using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.MediaSoup;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Producer
    {
        bool _stopTracks;
        bool _disableTrackOnPause;
        bool _zeroRtpOnPause;
        int _maxSpatialLayer;

        public event EventHandler OnClose;
        public event EventHandler OnTransportClosed;
        public event EventHandler OnTrackEnded;


        // Internal events.
        internal event EventHandler<IMediaStreamTrack> ReplaceTrackEvent;
        internal event EventHandler<int> SetMaxSpatialLayerEvent;
        internal event EventHandler<RTCRtpEncodingParameters> SetRtpEncodingParametersEvent;

        internal delegate Task<IRTCStatsReport> GetStatsDelegate(string producerLocalId);
        internal event GetStatsDelegate GetStatsEvent;

        public Producer(string id, string localId, IRTCRtpSender rtpSender, IMediaStreamTrack track, 
            RtpParameters rtpParameters, bool stopTracks, bool disableTrackOnPause, bool zeroRtpOnPause, 
            object appData)
        {
            Id = id;
            LocalId = localId;
            RtpSender = rtpSender;
            Track = track;
            RtpParameters = rtpParameters;
            _stopTracks = stopTracks;
            _disableTrackOnPause = disableTrackOnPause;
            _zeroRtpOnPause = zeroRtpOnPause;
            AppData = appData;

            Kind = track.Kind.ToMediaSoup();
            Paused = disableTrackOnPause ? !Track.Enabled : false;

            HandleTrack();
        }

        public string Id { get; }
        public string LocalId { get; }
        public bool Closed { get; private set; }
        public MediaKind Kind { get; }

        public IRTCRtpSender RtpSender { get; }
        public IMediaStreamTrack Track { get; private set; }
        public RtpParameters RtpParameters { get; }
        public bool Paused { get; private set; }
        public int MaxSpatialLayer { get; }
        public object AppData { get; }


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

            if (GetStatsEvent is not null)
                return await Task.Run(async () => await GetStatsEvent(LocalId));
            else
                return null;
            
            ///return await RtpSender.GetStats();
        }

        public void Pause()
        {
            if (Closed)
                return;

            Paused = true;

            if (_disableTrackOnPause)
                Track.Enabled = false;

            if (_zeroRtpOnPause)
                RtpSender.ReplaceTrack();
        }

        public void Resume()
        {
            if (Closed)
                return;

            Paused = false;

            if (_disableTrackOnPause)
                Track.Enabled = true;

            if (_zeroRtpOnPause)
                RtpSender.ReplaceTrack(Track);
        }

        public void ReplaceTrack(IMediaStreamTrack track)
        {
            if (Closed)
            {
                // This must be done here. Otherwise there is no chance to stop the given track.
                if (track is not null && _stopTracks)
                {
                    try { track.Stop(); }
                    catch { }
                }
                throw new Exception("closed");
            }
            else if (track is not null && track.ReadyState == MediaStreamTrackState.Ended)
            {
                throw new Exception("track ended");
            }

            // Do nothing if this is the same track as the current handled one.
            if (track == Track)
            {
                Console.WriteLine("replaceTrack() | same track, ignored");
                return;
            }

            if (!_zeroRtpOnPause || !Paused)
            {
                ReplaceTrackEvent?.Invoke(this, track);
            }

            // Destroy the previous track.
            DestroyTrack();

            // Set the new track.
            Track = track;

            // If this Producer was paused/resumed and the state of the new
            // track does not match, fix it.
            if (Track is not null && _disableTrackOnPause)
            {
                if (!Paused)
                    Track.Enabled = true;
                else if (Paused)
                    Track.Enabled = false;
            }

            // Handle the effective track.
            HandleTrack();
        }

        public void SetMaxSpatialLayer(int spatialLayer)
        {
            if (Closed)
                throw new Exception("closed");
            else if (Kind != MediaKind.Video)
                throw new Exception("not a video Producer");

            if (spatialLayer == _maxSpatialLayer)
                return;

            SetMaxSpatialLayerEvent?.Invoke(this, spatialLayer);
            _maxSpatialLayer = spatialLayer;

        }

        public void SetRtpEncodingParameters(RTCRtpEncodingParameters parameters)
        {
            if (Closed)
                throw new Exception("closed");

            SetRtpEncodingParametersEvent?.Invoke(this, parameters);
        }

        void HandleTrack()
        {
            Track.OnEnded += Producer_TrackEnded;
        }

        private void Producer_TrackEnded(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void DestroyTrack()
        {
            if (Track is null)
                return;

            try
            {
                Track.OnEnded -= Producer_TrackEnded;
                // Just stop the track unless the app set stopTracks: false.
                if (_stopTracks)
                    Track.Stop();
            }
            catch
            { }

        }
    }
}
