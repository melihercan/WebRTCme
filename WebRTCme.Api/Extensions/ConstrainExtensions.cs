using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public static class ConstrainExtensions
    {
        public static bool ToBool(this ConstrainBoolean constrainBoolean) =>
            // order: Ideal, Exact, Value, false
            constrainBoolean.Object?.Ideal ?? (constrainBoolean.Object?.Exact ?? (constrainBoolean.Value ?? false));
    }
}
