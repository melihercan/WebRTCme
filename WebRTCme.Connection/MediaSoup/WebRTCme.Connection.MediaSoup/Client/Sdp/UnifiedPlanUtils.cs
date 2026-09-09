using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp
{
    public static class UnifiedPlanUtils
    {
        public static RtpEncodingParameters[] GetRtpEncodings(MediaObject offerMediaObject)
        {
			List<uint> ssrcs = (offerMediaObject.MediaDescription.Attributes.Ssrcs ?? new List<Ssrc>())
				.Select(s => s.Id)
				.Distinct()
				.ToList();

			if (ssrcs.Count == 0)
				throw new Exception("XXXXno a=ssrc lines found");

			Dictionary<uint, uint?> ssrcToRtxSsrc = new();

			// First assume RTX is used.
			foreach (var line in offerMediaObject.MediaDescription.Attributes.SsrcGroups ?? new List<SsrcGroup>())
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

		// Adds multi-ssrc based simulcast into the given SDP media section offer.
		public static void AddLegacySimulcast(MediaObject offerMediaObject, int numStreams)
        {
			if (numStreams <= 1)
				throw new Exception("'numStreams must be greater than 1");

			// Get the SSRC.
			var ssrcMsidLine = (offerMediaObject.MediaDescription.Attributes.Ssrcs ?? new List<Ssrc>())
				.FirstOrDefault(line => line.Attribute == "msid");

			if (ssrcMsidLine is null)
				throw new Exception("'a=ssrc line with msid information not found");

			var values = ssrcMsidLine.Value.Split(' ');
			var streamId = values[0];
			var trackId = values.Length > 1 ? values[1] : null;
			var firstSsrc = ssrcMsidLine.Id;
			uint? firstRtxSsrc = null;

			// Get the SSRC for RTX.
			(offerMediaObject.MediaDescription.Attributes.SsrcGroups ?? new List<SsrcGroup>()).Any(line =>
			{
				if (line.Semantics != "FID")
					return false;

				if (uint.Parse(line.SsrcIds[0]) == firstSsrc)
				{
					firstRtxSsrc = uint.Parse(line.SsrcIds[1]);
					return true;
				}
				else
				{
					return false;
				}
			});

			var ssrcCnameLine = offerMediaObject.MediaDescription.Attributes.Ssrcs
				.FirstOrDefault(line => line.Attribute == "cname");

			if (ssrcCnameLine is null)
				throw new Exception("a=ssrc line with cname information not found");

			var cname = ssrcCnameLine.Value;
			List<uint> ssrcs = new();
			List<uint> rtxSsrcs = new();

			for (uint i = 0; i < numStreams; ++i)
			{
				ssrcs.Add(firstSsrc + i);

				if (firstRtxSsrc is not null)
					rtxSsrcs.Add((uint)(firstRtxSsrc + i));
			}

			offerMediaObject.MediaDescription.Attributes.SsrcGroups = new List<SsrcGroup>();
			offerMediaObject.MediaDescription.Attributes.Ssrcs = new List<Ssrc>();

			var x = ssrcs.Select(ssrc => ssrc.ToString()).ToArray();

			offerMediaObject.MediaDescription.Attributes.SsrcGroups.Add(new SsrcGroup 
			{
				Semantics = "SIM",
				SsrcIds = ssrcs.Select(ssrc => ssrc.ToString()).ToArray()
			});

			for (int i = 0; i < ssrcs.Count; ++i)
			{
				var ssrc = ssrcs[i];

				offerMediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
				{
					Id = ssrc,
					Attribute = "cname",
					Value = cname
				});

				offerMediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
				{
					Id = ssrc,
					Attribute = "msid",
					Value = $"{ streamId} { trackId}"
				});
			}

			for (int i = 0; i<rtxSsrcs.Count; ++i)
			{
				var ssrc = ssrcs[i];
				var rtxSsrc = rtxSsrcs[i];

				offerMediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
				{
					Id = rtxSsrc,
					Attribute = "cname",
					Value = cname
				});

				offerMediaObject.MediaDescription.Attributes.Ssrcs.Add(new Ssrc
				{
					Id = rtxSsrc,
					Attribute = "msid",
					Value = $"{streamId} ${ trackId}"
				});

				offerMediaObject.MediaDescription.Attributes.SsrcGroups.Add(new SsrcGroup
				{
					Semantics = "FID",
					SsrcIds = new string[] { $"{ ssrc} ${ rtxSsrc}" }
				});
			}
		}
	}
}
