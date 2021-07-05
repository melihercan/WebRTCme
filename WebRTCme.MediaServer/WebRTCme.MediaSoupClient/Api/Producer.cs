using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.MediaSoupClient.Enums;
using WebRTCme.MediaSoupClient.Extensions;

namespace WebRTCme.MediaSoupClient.Api
{
    public class Producer
    {
        bool _stopTracks;
        bool _disableTrackOnPause;
        bool _zeroRtpOnPause;

        public Producer(string id, string localId, IRTCRtpSender rtpSender, IMediaStreamTrack track, 
            RTCRtpParameters rtpParameters, bool stopTracks, bool disableTrackOnPause, bool zeroRtpOnPause, 
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
        }

        public string Id { get; }
        public string LocalId { get; }
        public bool Closed { get; private set; }
        public MediaKind Kind { get; }

        public IRTCRtpSender RtpSender { get; }
        public IMediaStreamTrack Track { get; }
        public RTCRtpParameters RtpParameters { get; }
        public bool Paused { get; }
        public int MaxSpatialLayer { get; }
        public object AppData { get; }

        public event EventHandler OnClose;
        public event EventHandler OnPause;
        public event EventHandler OnResume;
        public event EventHandler OnTrackEnded;
        public event EventHandler OnTransportClose;


        public void Close()
        {
            if (!Closed)
            {
                Closed = true;
                DestroyTrack();
                OnClose?.Invoke(this, EventArgs.Empty);
            }
        }

        public void TransportClosed()
        {
            if (!Closed)
            {
                Closed = true;
                DestroyTrack();
                OnTransportClose?.Invoke(this, EventArgs.Empty);
                OnClose?.Invoke(this, EventArgs.Empty);
            }
        }

        //Task<IRTCStatsReport> GetStats()
        //{
        //    if (!Closed)
        //    {
                
        //    }

        //}



        void HandleTrack()
        {
        }

        void DestroyTrack()
        {
            if (Track is not null && _stopTracks)
            {
                Track.Stop();
            }
        }
    }
}
