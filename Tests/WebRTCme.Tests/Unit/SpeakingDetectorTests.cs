
using WebRTCme.Connection.Services;

namespace WebRTCme.Tests.Unit;

/// <summary>
/// The voice-activity rule: whether a microphone level means somebody is speaking.
/// </summary>
/// <remarks>
/// This took three attempts on real hardware to get right, and the record of what went wrong is
/// what most of these tests pin. The levels used are the ones measured on 2026-09-14 from the two
/// ends of a real call - an Android phone and a Mac mini webcam - because the whole reason the rule
/// is a ratio rather than a constant is that those two devices are a factor of six apart.
/// </remarks>
public class SpeakingDetectorTests
{
    static readonly DateTime T0 = new(2026, 9, 14, 12, 0, 0, DateTimeKind.Utc);

    // Measured, not invented. Android: silence 0.0002-0.0007, speech 0.0023-0.0063.
    // 0.0003 is chosen from within that range because it reproduces the recorded outcome: Android
    // settled at the absolute minimum threshold, since 3 x 0.0003 falls under the floor.
    const double AndroidSilence = 0.0003;
    const double AndroidSpeech = 0.0045;

    // Mac mini webcam: silence 0.0004-0.0012, speech peaks around 0.037. 0.0005 gives a threshold
    // of 0.0015, inside the 0.0014-0.0016 the Mac was actually observed to settle on.
    const double MacSilence = 0.0005;
    const double MacSpeech = 0.030;

    static SpeakingDetector Settled(double silence, out DateTime now)
    {
        var detector = new SpeakingDetector();
        now = T0;

        // Twenty quiet samples is well past the ~4 second settling time at a 400ms interval.
        for (var i = 0; i < 20; i++)
        {
            now += TimeSpan.FromMilliseconds(400);
            detector.Sample(silence, now, outgoingAudioEnabled: true);
        }

        return detector;
    }

    [Fact]
    public void Starts_at_the_floor_before_any_sample()
    {
        new SpeakingDetector().Threshold.Should().Be(SpeakingDetector.LevelFloor);
        new SpeakingDetector().NoiseFloor.Should().Be(double.NaN);
    }

    /// <summary>
    /// The fault that started all this: a single constant threshold. 0.01 sits above everything an
    /// Android phone produces, so that peer reported speaking never - while the Mac, whose speech
    /// peaks at 0.037, reported it fine. Both devices must work with the same code.
    /// </summary>
    [Theory]
    [InlineData(AndroidSilence, AndroidSpeech)]
    [InlineData(MacSilence, MacSpeech)]
    public void Detects_speech_on_a_device_after_settling_to_its_own_noise_floor(double silence, double speech)
    {
        var detector = Settled(silence, out var now);

        detector.Sample(speech, now + TimeSpan.FromMilliseconds(400), outgoingAudioEnabled: true)
                .Should().BeTrue();
    }

    [Theory]
    [InlineData(AndroidSilence)]
    [InlineData(MacSilence)]
    public void Stays_quiet_through_silence_on_either_device(double silence)
    {
        var detector = Settled(silence, out var now);

        detector.Sample(silence, now + TimeSpan.FromMilliseconds(400), outgoingAudioEnabled: true)
                .Should().BeFalse();
    }

    /// <summary>
    /// The two ends of one call settle on visibly different thresholds - Android at the absolute
    /// minimum, the Mac adapting upwards - which is the point of tracking the floor per device.
    /// </summary>
    [Fact]
    public void Two_devices_settle_on_different_thresholds()
    {
        var android = Settled(AndroidSilence, out _);
        var mac = Settled(MacSilence, out _);

        android.Threshold.Should().Be(SpeakingDetector.LevelFloor,
            "3 x 0.0003 is below the floor, so the floor wins - as observed on the phone");
        mac.Threshold.Should().BeApproximately(0.0015, 0.0001,
            "3 x 0.0005 clears the floor - and 0.0014-0.0016 is what the Mac was observed to settle on");
    }

    /// <summary>
    /// Without a floor under the floor, a microphone reporting digital silence would make any faint
    /// sound count as speech.
    /// </summary>
    [Fact]
    public void Never_drops_the_threshold_below_the_floor()
    {
        var detector = Settled(silence: 0.0, out var now);

        detector.Threshold.Should().Be(SpeakingDetector.LevelFloor);
        detector.Sample(0.0005, now + TimeSpan.FromMilliseconds(400), outgoingAudioEnabled: true)
                .Should().BeFalse("a faint sound under the floor is not speech");
    }

    /// <summary>
    /// Speech must not move the noise floor. Letting it would walk the threshold up mid-sentence
    /// until the speaker fell below their own floor and the flag dropped while they were still
    /// talking.
    /// </summary>
    [Fact]
    public void Speech_does_not_raise_the_noise_floor()
    {
        var detector = Settled(MacSilence, out var now);
        var floorBefore = detector.NoiseFloor;

        for (var i = 0; i < 10; i++)
        {
            now += TimeSpan.FromMilliseconds(400);
            detector.Sample(MacSpeech, now, outgoingAudioEnabled: true);
        }

        detector.NoiseFloor.Should().Be(floorBefore);
    }

    /// <summary>
    /// The hangover, measured rather than chosen: at 900ms the flag fell and rose twice inside one
    /// spoken sentence, with quiet stretches of 1.2 to 1.7 seconds. A gap shorter than the hangover
    /// must not drop the flag.
    /// </summary>
    [Fact]
    public void Holds_the_flag_through_a_pause_shorter_than_the_hangover()
    {
        var detector = Settled(MacSilence, out var now);

        now += TimeSpan.FromMilliseconds(400);
        detector.Sample(MacSpeech, now, outgoingAudioEnabled: true).Should().BeTrue();

        // A 1.7 second gap - the longest measured inside a sentence.
        now += TimeSpan.FromMilliseconds(1700);
        detector.Sample(MacSilence, now, outgoingAudioEnabled: true)
                .Should().BeTrue("a pause inside a sentence must not drop the flag");
    }

    [Fact]
    public void Drops_the_flag_once_the_hangover_expires()
    {
        var detector = Settled(MacSilence, out var now);

        now += TimeSpan.FromMilliseconds(400);
        detector.Sample(MacSpeech, now, outgoingAudioEnabled: true).Should().BeTrue();

        now += SpeakingDetector.Hangover + TimeSpan.FromMilliseconds(1);
        detector.Sample(MacSilence, now, outgoingAudioEnabled: true).Should().BeFalse();
    }

    /// <summary>
    /// "Muted and speaking" is a contradiction a peer should never receive, so mute forces the flag
    /// down even mid-sentence.
    /// </summary>
    [Fact]
    public void Muting_drops_the_flag_even_while_speech_is_still_within_the_hangover()
    {
        var detector = Settled(MacSilence, out var now);

        now += TimeSpan.FromMilliseconds(400);
        detector.Sample(MacSpeech, now, outgoingAudioEnabled: true).Should().BeTrue();

        now += TimeSpan.FromMilliseconds(400);
        detector.Sample(MacSpeech, now, outgoingAudioEnabled: false).Should().BeFalse();
    }

    /// <summary>
    /// The bug that made one end of a call report speaking permanently while the other reported it
    /// never. The level was formatted through the current culture and parsed as invariant, so on a
    /// comma-decimal machine 0.0021 became "0,0021", the comma read as a group separator, and the
    /// level arrived as twenty-one thousand.
    /// <para>
    /// The parsing fix lives in the sampler, and how far out the value landed depended on the level
    /// and the culture - orders of magnitude either way. What this pins is the consequence at the
    /// rule, which is the same whatever the exact number: a level that large is not speech, it is
    /// nonsense - and it must not be allowed to settle as a noise floor, or the detector would be
    /// deaf afterwards.
    /// </para>
    /// </summary>
    [Fact]
    public void A_culture_mangled_level_does_not_poison_the_noise_floor()
    {
        // A group separator swallowed by an invariant parse moves the decimal point; the result is
        // some absurd multiple of the real level rather than one specific number.
        const double mangled = 21_000;

        var detector = Settled(MacSilence, out var now);
        var floorBefore = detector.NoiseFloor;

        now += TimeSpan.FromMilliseconds(400);
        detector.Sample(mangled, now, outgoingAudioEnabled: true).Should().BeTrue();

        detector.NoiseFloor.Should().Be(floorBefore,
            "a level above the threshold never moves the floor, however absurd it is");
    }

    /// <summary>
    /// The floor follows the room rather than jumping to it: a fan starting should settle in over
    /// a few seconds, not in one sample.
    /// </summary>
    [Fact]
    public void The_noise_floor_follows_a_rising_room_gradually()
    {
        var detector = Settled(0.0008, out var now);
        var quiet = detector.NoiseFloor;

        now += TimeSpan.FromMilliseconds(400);
        detector.Sample(0.0016, now, outgoingAudioEnabled: true);

        detector.NoiseFloor.Should().BeGreaterThan(quiet).And.BeLessThan(0.0016,
            "one sample moves the floor a tenth of the way, not all of it");
    }

    [Fact]
    public void The_first_quiet_sample_sets_the_floor_outright()
    {
        var detector = new SpeakingDetector();

        detector.Sample(0.0009, T0, outgoingAudioEnabled: true);

        detector.NoiseFloor.Should().Be(0.0009, "there is nothing yet to average against");
    }
}
