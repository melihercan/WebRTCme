using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public static class Utils
    {
        public static T Clone<T>(T data, T defaultData)
        {
            if (data is null)
                return defaultData;

            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(data));
        }

        public static int GenerateRandomNumber()
        {
            return Convert.ToInt32(new Random().NextDouble() * 10000000);
        }

    }
}
