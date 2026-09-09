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
        public event EventHandlerAsync<IMediaStreamTrack> OnReplaceTrackAsync;
        public event EventHandlerAsync<int> OnSetMaxSpatialLayerAsync;
        public event EventHandlerAsync<RTCRtpEncodingParameters> OnSetRtpEncodingParametersAsync;
        public event EventHandlerAsync<EventArgs, IRTCStatsReport> OnGetStatsAsync;

        public Producer(string id, string localId, IRTCRtpSender rtpSender, IMediaStreamTrack track, 
            RtpParameters rtpParameters, bool stopTracks, bool disableTrackOnPause, bool zeroRtpOnPause,
            Dictionary<string, object> appData)
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

        public async Task<IRTCStatsReport> GetStatsAsync()
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

            if (_disableTrackOnPause)
                Track.Enabled = false;

            if (_zeroRtpOnPause)
                OnReplaceTrackAsync?.Invoke(this, null);
        }

        public void Resume()
        {
            if (Closed)
                return;

            Paused = false;

            if (_disableTrackOnPause)
                Track.Enabled = true;

            if (_zeroRtpOnPause)
                OnReplaceTrackAsync?.Invoke(this, Track);
        }

        public async Task ReplaceTrackAsync(IMediaStreamTrack track)
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
                await OnReplaceTrackAsync?.Invoke(this, track);
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

        public async Task SetMaxSpatialLayerAsync(int spatialLayer)
        {
            if (Closed)
                throw new Exception("closed");
            else if (Kind != MediaKind.Video)
                throw new Exception("not a video Producer");

            if (spatialLayer == _maxSpatialLayer)
                return;

            await OnSetMaxSpatialLayerAsync?.Invoke(this, spatialLayer);
            _maxSpatialLayer = spatialLayer;

        }

        public async Task SetRtpEncodingParameters(RTCRtpEncodingParameters parameters)
        {
            if (Closed)
                throw new Exception("closed");

            await OnSetRtpEncodingParametersAsync?.Invoke(this, parameters);
        }

        void HandleTrack()
        {
            Track.OnEnded += Producer_TrackEnded;
        }

        void Producer_TrackEnded(object sender, EventArgs e)
        {
            OnTrackEnded?.Invoke(this, EventArgs.Empty);
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
