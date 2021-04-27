using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebRTCme
{
    public static class ApiExtensions
    {
        public static int NetworkCost(this IRTCIceCandidate iceCandidate)
        {
            var array = iceCandidate.Candidate
                .Replace("candidate:", string.Empty)
                .Split(' ');
            var index = array.ToList().FindIndex(s => s.Equals("network-cost", StringComparison.OrdinalIgnoreCase));
            if (index == -1)
                return -1;
            else
                return Convert.ToInt32(array[index + 1]);
        }
    }
}
