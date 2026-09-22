using System;

namespace WebRTCme
{
    /// <summary>
    /// Keeps libwebrtc's VoIP audio unit initialised across calls, instead of letting it be torn
    /// down and rebuilt for each one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>What this is for.</b> Rebuilding the audio unit is what provokes the abort reported as
    /// WebRTCme#49: an app that ends a call and immediately starts another died on the 33rd round,
    /// on a libwebrtc worker thread, inside Apple's audio code. The path is
    /// <c>UpdateAudioUnit</c> -&gt; <c>RTCLog(@"%@", session)</c> -&gt;
    /// <c>-[RTCAudioSession description]</c> -&gt; <c>-[AVAudioSession IOBufferDuration]</c> -&gt;
    /// <c>getATDefaultDeviceAggregateID</c>, where an aggregate-device object's last shared
    /// reference is dropped and libmalloc finds it already gone.
    /// </para>
    /// <para>
    /// The <c>description</c> call builds a debug string that nothing reads: libwebrtc's
    /// <c>RTCLogFormat</c> formats its arguments <i>before</i> checking the severity, so the eleven
    /// audio-session properties are queried whatever the log level, and no application setting can
    /// switch it off. But it sits inside <c>if (should_initialize_audio_unit)</c>, so a process that
    /// never re-initialises the unit never reaches it.
    /// </para>
    /// <para>
    /// <b>How.</b> <c>useManualAudio</c> stops libwebrtc initialising the unit by itself, and
    /// <c>isAudioEnabled</c> becomes the permission it asks instead. Turned on once and left on,
    /// the unit is built when the first call needs it and stays built - so ending a call no longer
    /// uninitialises it, and starting the next no longer rebuilds it.
    /// </para>
    /// <para>
    /// <b>The cost, and why this is opt-in.</b> The audio unit stays live between calls, which
    /// means the microphone stays open and the system's recording indicator stays lit while the app
    /// is idle. That is a real change in what a user sees and is not a reasonable default, so
    /// nothing here happens unless an application asks for it. Off, the behaviour is exactly what it
    /// has always been.
    /// </para>
    /// <para>
    /// Toggling <c>isAudioEnabled</c> per call would <i>not</i> work: upstream documents that
    /// setting it to NO stops and uninitialises the unit, which is the very thing being avoided.
    /// The only useful setting is on and left alone.
    /// </para>
    /// <para>
    /// <b>This is a mitigation, not a fix.</b> The defect is upstream - a debug string built
    /// unconditionally on a teardown path - and it wants reporting there. This makes it
    /// unreachable; it does not make it untrue.
    /// </para>
    /// </remarks>
    public static class AppleAudioUnit
    {
        static readonly object Gate = new();
        static bool _kept;

        /// <summary>
        /// Whether the audio unit is currently being kept initialised between calls.
        /// </summary>
        public static bool IsKeptInitialized
        {
            get { lock (Gate) return _kept; }
        }

        /// <summary>
        /// Turns the behaviour described on this class on or off. Safe to call more than once;
        /// repeated calls with the same value do nothing.
        /// </summary>
        /// <param name="keep">
        /// <c>true</c> to keep the audio unit initialised between calls, holding the microphone
        /// open while the app is idle. <c>false</c> to hand control back to libwebrtc, which is
        /// the default.
        /// </param>
        public static void KeepInitialized(bool keep)
        {
            lock (Gate)
            {
                if (_kept == keep)
                    return;

                var session = Webrtc.RTCAudioSession.SharedInstance();

                if (keep)
                {
                    // Order matters. useManualAudio first, or the permission below is read while
                    // libwebrtc is still managing the unit itself and means nothing.
                    session.UseManualAudio = true;
                    session.IsAudioEnabled = true;
                }
                else
                {
                    // And the reverse on the way out: withdraw the permission, which stops and
                    // uninitialises the unit, before handing management back.
                    session.IsAudioEnabled = false;
                    session.UseManualAudio = false;
                }

                _kept = keep;
            }
        }
    }
}
