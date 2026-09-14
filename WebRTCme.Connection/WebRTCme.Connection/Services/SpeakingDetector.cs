using System;

namespace WebRTCme.Connection.Services
{
    /// <summary>
    /// Decides whether a microphone level means somebody is speaking.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Speech is judged against the room, not against a fixed number. A fixed one was tried first
    /// and cannot work across these devices. Measured 2026-09-14, media-source audioLevel, the same
    /// build on both ends of one call:
    /// </para>
    /// <code>
    ///                     silence            speech
    ///   Android phone     0.0002 - 0.0007    0.0023 - 0.0063
    ///   Mac mini webcam   0.0004 - 0.0012    up to 0.037
    /// </code>
    /// <para>
    /// The old 0.01 sat above everything Android produces, so that peer never reported speaking at
    /// all. Anything low enough to catch Android's quietest speech has under a factor of two over
    /// the Mac's idle level, which is not a margin. The microphones differ by about 2x in noise
    /// floor and 6x in speech peak, and no single constant fits both.
    /// </para>
    /// <para>
    /// What is stable across both is the <em>ratio</em>: speech runs three to fifty times the
    /// floor. So the floor is tracked per device and the threshold follows it.
    /// </para>
    /// <para>
    /// This is deliberately a pure class: it holds the decision and none of the I/O. It is fed a
    /// level and the current time and answers a bool, so the rule can be tested without a
    /// microphone, a peer connection or a two-second wait. The sampling loop that calls it lives in
    /// <see cref="SignalingConnection"/>.
    /// </para>
    /// </remarks>
    class SpeakingDetector
    {
        /// <summary>Speech is taken to be this many times the tracked noise floor.</summary>
        public const double LevelFactor = 3.0;

        /// <summary>
        /// A floor for the floor, for a microphone that reports digital silence: without it a floor
        /// near zero makes any faint sound "speech".
        /// </summary>
        public const double LevelFloor = 0.001;

        /// <summary>
        /// How fast the tracked noise floor follows the room, per sample. About four seconds at the
        /// 400ms sample interval - slow enough to ignore a cough, quick enough to settle when a fan
        /// starts.
        /// </summary>
        public const double NoiseFloorAdaption = 0.1;

        /// <summary>
        /// How long the flag is held after the level drops below the threshold. Speech is full of
        /// gaps, and without a hold-off the flag flickers several times a sentence - which is a
        /// worse thing to put in front of a viewer than a flag that lags by a beat.
        /// <para>
        /// Two seconds, from measurement rather than taste: at 900ms the flag still fell and rose
        /// twice inside a single spoken sentence, with the quiet stretches running 1.2 to 1.7
        /// seconds. Anything under about 1.8s reproduces that.
        /// </para>
        /// </summary>
        public static readonly TimeSpan Hangover = TimeSpan.FromSeconds(2);

        double _noiseFloor = double.NaN;
        DateTime _lastHeard = DateTime.MinValue;

        /// <summary>
        /// The tracked noise floor, or <see cref="double.NaN"/> before any quiet sample has been
        /// seen. Exposed so the sampler can log why a decision went the way it did.
        /// </summary>
        public double NoiseFloor => _noiseFloor;

        /// <summary>The level a sample has to exceed to count as speech.</summary>
        public double Threshold => double.IsNaN(_noiseFloor)
            ? LevelFloor
            : Math.Max(LevelFloor, _noiseFloor * LevelFactor);

        /// <summary>
        /// Feeds one microphone reading in and returns whether this client should now be reported
        /// as speaking.
        /// </summary>
        /// <param name="level">The microphone level, as read from media-source statistics.</param>
        /// <param name="now">The time of this sample. Passed in rather than read from the clock so
        /// the hangover can be tested without waiting for it.</param>
        /// <param name="outgoingAudioEnabled">False when muted. A disabled track reports a level of
        /// zero so mute would drop the flag on its own, but it is forced anyway: "muted and
        /// speaking" is a contradiction a peer should never receive.</param>
        public bool Sample(double level, DateTime now, bool outgoingAudioEnabled)
        {
            if (level > Threshold)
            {
                _lastHeard = now;
            }
            else
            {
                // Only quiet samples move the floor. Letting speech raise it would walk the
                // threshold up mid-sentence until the speaker fell below their own floor and the
                // flag dropped while they were still talking.
                _noiseFloor = double.IsNaN(_noiseFloor)
                    ? level
                    : _noiseFloor + (level - _noiseFloor) * NoiseFloorAdaption;
            }

            return outgoingAudioEnabled && now - _lastHeard < Hangover;
        }
    }
}
