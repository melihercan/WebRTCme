using System;
using System.Linq;

namespace WebRTCme
{
    /// <summary>
    /// The part of <see cref="MediaTrackConstraints"/> that a camera can actually be opened with.
    /// </summary>
    /// <remarks>
    /// getUserMedia's constraints are a union of unions: a bare value, an array of values, an
    /// <c>exact</c>, an <c>ideal</c>, a min/max range. Every platform that opens a camera needs the
    /// same four answers out of that - which device, which way it faces, what size, how fast - so
    /// the reading happens once here instead of three times over in three different shapes.
    ///
    /// Exact and ideal are kept apart because they mean different things to whoever opens the
    /// camera: an exact constraint that cannot be satisfied is an error, an ideal one is a
    /// preference to drop quietly. Nothing here enforces that - it only reports which was asked
    /// for.
    /// </remarks>
    public sealed class VideoConstraints
    {
        /// <summary>Nothing was asked for; every platform's own default applies.</summary>
        public static readonly VideoConstraints None = new();

        /// <summary>The requested device id, or null if the caller did not name one.</summary>
        public string DeviceId { get; init; }

        /// <summary><see cref="DeviceId"/> came from <c>exact</c>, so failing to match it is an error.</summary>
        public bool DeviceIdIsExact { get; init; }

        /// <summary>"user", "environment", "left" or "right" per the spec, or null.</summary>
        public string FacingMode { get; init; }

        /// <summary><see cref="FacingMode"/> came from <c>exact</c>.</summary>
        public bool FacingModeIsExact { get; init; }

        public int? Width { get; init; }

        public int? Height { get; init; }

        public int? FrameRate { get; init; }

        /// <summary>True when the caller asked for a specific capture size.</summary>
        public bool HasSize => Width.HasValue && Height.HasValue;

        /// <summary>
        /// Reads what a camera can act on out of a constraints object, which may be null.
        /// </summary>
        public static VideoConstraints From(MediaTrackConstraints constraints)
        {
            if (constraints is null)
                return None;

            var (deviceId, deviceIdIsExact) = FirstString(constraints.DeviceId);
            var (facingMode, facingModeIsExact) = FirstString(constraints.FacingMode);

            return new VideoConstraints
            {
                DeviceId = deviceId,
                DeviceIdIsExact = deviceIdIsExact,
                FacingMode = facingMode,
                FacingModeIsExact = facingModeIsExact,
                Width = Count(constraints.Width),
                Height = Count(constraints.Height),
                FrameRate = Rate(constraints.FrameRate)
            };
        }

        /// <summary>
        /// Reads a video track's constraints straight out of the stream constraints.
        /// </summary>
        public static VideoConstraints From(MediaStreamConstraints constraints) =>
            From(constraints?.Video?.Object);

        /// <summary>
        /// The first usable string, and whether it was demanded rather than preferred.
        /// </summary>
        /// <remarks>
        /// Exact is read before the bare value and ideal, so that a caller who says both gets the
        /// binding one. An array means "any of these"; the first is as good an answer as any,
        /// since the caller asked for no order among them.
        /// </remarks>
        static (string Value, bool IsExact) FirstString(ConstrainDOMString constrain)
        {
            if (constrain is null)
                return (null, false);

            var exact = FromUnion(constrain.Exact);
            if (exact is not null)
                return (exact, true);

            if (!string.IsNullOrEmpty(constrain.Value))
                return (constrain.Value, false);

            var first = constrain.Array?.FirstOrDefault(value => !string.IsNullOrEmpty(value));
            if (first is not null)
                return (first, false);

            return (FromUnion(constrain.Ideal), false);

            static string FromUnion(ConstrainDOMStringUnion union) =>
                union is null
                    ? null
                    : !string.IsNullOrEmpty(union.Value)
                        ? union.Value
                        : union.Array?.FirstOrDefault(value => !string.IsNullOrEmpty(value));
        }

        /// <summary>
        /// A pixel count from any of the forms a <see cref="ConstrainULong"/> can take.
        /// </summary>
        /// <remarks>
        /// Max before min: a caller who gives only a range and no target is asking to stay within
        /// it, and the top of the range is the best picture that satisfies that. Taking the bottom
        /// would honour the constraint and still look like a bug.
        /// </remarks>
        static int? Count(ConstrainULong constrain)
        {
            if (constrain is null)
                return null;

            var value = constrain.Value
                ?? constrain.Object?.Exact
                ?? constrain.Object?.Ideal
                ?? constrain.Object?.Max
                ?? constrain.Object?.Min;

            return value is null || value.Value > int.MaxValue ? null : (int)value.Value;
        }

        static int? Rate(ConstrainDouble constrain)
        {
            if (constrain is null)
                return null;

            var value = constrain.Value
                ?? constrain.Object?.Exact
                ?? constrain.Object?.Ideal
                ?? constrain.Object?.Max
                ?? constrain.Object?.Min;

            return value is null || value.Value <= 0 || value.Value > int.MaxValue
                ? null
                : (int)Math.Round(value.Value);
        }
    }
}
