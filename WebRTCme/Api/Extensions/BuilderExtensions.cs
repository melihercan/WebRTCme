using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public static class BuilderExtensions
    {
        public static T If<T>(this T t, bool condition, Func<T, T> builder) where T : class
        {
            if (condition)
                return builder(t);
            return t;
        }
    }
}
