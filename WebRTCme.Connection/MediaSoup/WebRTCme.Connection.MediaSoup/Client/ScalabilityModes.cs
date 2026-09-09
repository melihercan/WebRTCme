using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{

    public static class ScalabilityModes
    {
        static Regex ScalabilityModeRegex = new("^[LS]([1-9]\\d{0,1})T([1-9]\\d{0,1})");

		public static ScalabilityMode Parse(string scalabilityMode = "")
		{
			scalabilityMode ??= string.Empty;
			var match = ScalabilityModeRegex.Match(scalabilityMode);
			if (match.Success && match.Groups.Count >= 2)
			{
				return new ScalabilityMode 
				{
					SpatialLayers = int.Parse(match.Groups[1].Value),
					TemporalLayers = int.Parse(match.Groups[2].Value)
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
