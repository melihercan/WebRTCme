using System;
using System.Collections.Generic;
using System.Linq;
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

		// Adds multi-ssrc based simulcast into the given SDP media section offer.
		public static void AddLegacySimulcast(MediaObject offerMediaObject, int numStreams)
        {
			if (numStreams <= 1)
				throw new Exception("'numStreams must be greater than 1");

			// Get the SSRC.
			var ssrcMsidLine = (offerMediaObject.Ssrcs ?? new List<Ssrc>())
				.FirstOrDefault(line => line.AttributesAndValues.Any(aav => aav == "msid"));

			if (ssrcMsidLine is null)
				throw new Exception("'a=ssrc line with msid information not found");


/****
			const [streamId, trackId] = ssrcMsidLine.value.split(' ');
			const firstSsrc = ssrcMsidLine.id;
			let firstRtxSsrc;

			// Get the SSRC for RTX.
			(offerMediaObject.ssrcGroups || [])
				.some((line: any) =>
		{
				if (line.semantics !== 'FID')
					return false;

				const ssrcs = line.ssrcs.split(/\s +/);

				if (Number(ssrcs[0]) === firstSsrc)
				{
					firstRtxSsrc = Number(ssrcs[1]);

					return true;
				}
				else
				{
					return false;
				}
			});

			const ssrcCnameLine = offerMediaObject.ssrcs
				.find((line: any) => line.attribute === 'cname');

			if (!ssrcCnameLine)
				throw new Error('a=ssrc line with cname information not found');

			const cname = ssrcCnameLine.value;
			const ssrcs = [];
			const rtxSsrcs = [];

			for (let i = 0; i < numStreams; ++i)
			{
				ssrcs.push(firstSsrc + i);

				if (firstRtxSsrc)
					rtxSsrcs.push(firstRtxSsrc + i);
			}

			offerMediaObject.ssrcGroups = [];
			offerMediaObject.ssrcs = [];

			offerMediaObject.ssrcGroups.push(
		{
			semantics: 'SIM',
			ssrcs: ssrcs.join(' ')
		});

			for (let i = 0; i < ssrcs.length; ++i)
			{
				const ssrc = ssrcs[i];

				offerMediaObject.ssrcs.push(
					{
				id: ssrc,
				attribute: 'cname',
				value: cname
			});

			offerMediaObject.ssrcs.push(
			{
			id: ssrc,
				attribute: 'msid',
				value: `${ streamId} ${ trackId}`
			});
		}

	for (let i = 0; i<rtxSsrcs.length; ++i)
	{
		const ssrc = ssrcs[i];
		const rtxSsrc = rtxSsrcs[i];

		offerMediaObject.ssrcs.push(
			{
				id        : rtxSsrc,
				attribute : 'cname',
				value     : cname
	});

		offerMediaObject.ssrcs.push(
			{
				id        : rtxSsrc,
				attribute : 'msid',
				value     : `${streamId
} ${ trackId}`
			});

offerMediaObject.ssrcGroups.push(
			{
semantics: 'FID',
				ssrcs: `${ ssrc} ${ rtxSsrc}`
			});
	}
****/
        }
	}
}
