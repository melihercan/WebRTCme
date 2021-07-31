using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{

    public static class ScalabilityModes
    {
        static Regex ScalabilityModeRegex = new("'^[LS]([1-9]\\d{0,1})T([1-9]\\d{0,1})");

		public static ScalabilityMode Parse(string scalabilityMode = "")
		{
			var matches = ScalabilityModeRegex.Matches(scalabilityMode);
			if (matches.Count > 0)
			{
				return new ScalabilityMode 
				{
					SpatialLayers = int.Parse(matches[1].Value),
					TemporalLayers = int.Parse(matches[2].Value)
				};
			}
			else
			{
				return new ScalabilityMode 
				{
					SpatialLayers = 1,
					TemporalLayers = 1
				};
			}
		}

    }
}
