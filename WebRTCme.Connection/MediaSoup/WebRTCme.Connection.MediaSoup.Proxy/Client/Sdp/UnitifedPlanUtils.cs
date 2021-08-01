using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    public static class UnitifedPlanUtils
    {
        public static RtpEncodingParameters[] GetRtpEncodings(MediaObject offerMediaObject)
        {
			List<uint> ssrcs = new();

			foreach (var line in offerMediaObject.Ssrcs ?? new List<Ssrc>())
			{
				var ssrc = line.Id;
				ssrcs.Add(ssrc);
			}

			if (ssrcs.Count == 0)
				throw new Exception("no a=ssrc lines found");

			Dictionary<uint, uint?> ssrcToRtxSsrc = new();

			// First assume RTX is used.
			foreach (var line in offerMediaObject.SsrcGroups ?? new List<SsrcGroup>())
			{
				if (line.Semantics != "FID")
					continue;

				var ssrc = uint.Parse(line.SsrcIds[0]);
				var rtxSsrc = uint.Parse(line.SsrcIds[1]);

				if (ssrcs.Contains(ssrc))
				{
					// Remove both the SSRC and RTX SSRC from the set so later we know that they
					// are already handled.
					ssrcs.Remove(ssrc);
					ssrcs.Remove(rtxSsrc);

					// Add to the map.
					ssrcToRtxSsrc.Add(ssrc, rtxSsrc);
				}
			}

			// If the set of SSRCs is not empty it means that RTX is not being used, so take
			// media SSRCs from there.
			foreach (var ssrc in ssrcs)
			{
				// Add to the map.
				ssrcToRtxSsrc.Add(ssrc, null);
			}

			List<RtpEncodingParameters>  encodings = new();

			foreach (var item in ssrcToRtxSsrc)
			{
				RtpEncodingParameters encoding = new();
				encoding.Ssrc = item.Key;
				if (item.Value is not null)
					encoding.Rtx = new() { Ssrc = item.Value };
				encodings.Add(encoding);
			}

			return encodings.ToArray();
		}
	}
}
