using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WebRTCme.Middleware
{
    /// <summary>
    /// Everything a view needs to render one tile in a call.
    /// </summary>
    /// <remarks>
    /// <para>Split in two, and the split matters. <see cref="Stream"/>, <see cref="Label"/>,
    /// <see cref="AudioMuted"/> and the rest describe how to <em>attach</em> a stream to a view:
    /// they are read once when the tile is built and a change to any of them means building it
    /// again. <see cref="PeerAudioMuted"/>, <see cref="PeerVideoMuted"/> and
    /// <see cref="PeerSpeaking"/> describe what the peer is <em>doing</em>, they change constantly,
    /// and they raise <see cref="PropertyChanged"/> so a view can follow them without being
    /// rebuilt.</para>
    /// <para>That is not a stylistic preference. <c>MediaStreamManager.Add</c> and
    /// <c>Update</c> both remove the tile and insert it again - they have to, because MAUI's
    /// <c>BindableLayout</c> ignores a <c>Replace</c> and keeps the stream the tile was first
    /// given. Every one of those round trips tears down and recreates the platform video
    /// renderer. Routing "this peer started speaking" through that path would recreate the video
    /// view on every phrase, which is the same fault the fixed-height status row on the call pages
    /// exists to avoid, one layer down and considerably more expensive.</para>
    /// <para>So: anything that changes while a call is running is a property on this object, set
    /// in place. Anything that identifies the stream goes through the manager.</para>
    /// </remarks>
    public class MediaStreamParameters : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IMediaStream Stream { get; set; }

        public string Label { get; set; }

        public bool Hangup { get; set; }

        /// <summary>
        /// Whether this view's own audio is silenced - the local preview sets it to stop the echo.
        /// Nothing to do with whether the peer muted a microphone; see <see cref="PeerAudioMuted"/>.
        /// </summary>
        public bool AudioMuted { get; set; }

        public bool VideoMuted { get; set; }

        public CameraType CameraType { get; set; }

        public bool ShowControls { get; set; }

        /// <summary>
        /// Whether this tile is the local preview rather than a peer.
        /// </summary>
        /// <remarks>
        /// Views scale a self-view to fit and a peer to fill: cropping someone else is cosmetic,
        /// while cropping yourself hides what you are actually sending, which is the one thing a
        /// self-view is for.
        /// </remarks>
        public bool IsLocal { get; set; }

        bool _peerAudioMuted;
        bool _peerVideoMuted;
        bool _peerSpeaking;

        /// <summary>
        /// Whether the peer has muted its microphone.
        /// </summary>
        public bool PeerAudioMuted
        {
            get => _peerAudioMuted;
            set => Set(ref _peerAudioMuted, value);
        }

        /// <summary>
        /// Whether the peer has turned its camera off.
        /// </summary>
        public bool PeerVideoMuted
        {
            get => _peerVideoMuted;
            set => Set(ref _peerVideoMuted, value);
        }

        /// <summary>
        /// Whether the peer is talking right now.
        /// </summary>
        public bool PeerSpeaking
        {
            get => _peerSpeaking;
            set => Set(ref _peerSpeaking, value);
        }

        void Set(ref bool field, bool value, [CallerMemberName] string name = null)
        {
            if (field == value)
                return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
