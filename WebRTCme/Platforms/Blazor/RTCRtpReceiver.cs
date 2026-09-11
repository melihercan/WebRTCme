using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using System.Linq;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCRtpReceiver : NativeBase, IRTCRtpReceiver
    {
        public RTCRtpReceiver(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IMediaStreamTrack Track =>
            new MediaStreamTrack(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "track"));

        public IRTCDtlsTransport Transport =>
            new RTCDtlsTransport(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "transport"));

        public double? JitterBufferTarget
        {
            get => GetNativeProperty<double?>("jitterBufferTarget");
            set => SetNativeProperty("jitterBufferTarget", value);
        }

        public RTCRtpContributingSource[] GetContributingSources()
        {
            var rtpContributingSources = new List<RTCRtpContributingSource>();
            var jsObjectRefGetContributingSources =
                JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getContributingSources");
            var jsObjectRefRtpContributingSourceArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetContributingSources);
            foreach (var jsObjectRefRtpContributingSource in jsObjectRefRtpContributingSourceArray)
            {
                rtpContributingSources.Add(JsRuntime.GetJsPropertyValue<RTCRtpContributingSource>(
                    jsObjectRefRtpContributingSource, null));
                JsRuntime.DeleteJsObjectRef(jsObjectRefRtpContributingSource.JsObjectRefId);
            }
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetContributingSources.JsObjectRefId);
            return rtpContributingSources.ToArray();
        }

        public RTCRtpReceiveParameters GetParameters() => JsRuntime.CallJsMethodWithContent<RTCRtpReceiveParameters>(
            NativeObject, "getParameters");

        public async Task<IRTCStatsReport> GetStats() =>
            (await JsRuntime.GetJsStatsAsync(NativeObject)).ToStatsReport();

        public RTCRtpSynchronizationSource[] GetSynchronizationSources()
        {
            var rtpSynchronizationSources = new List<RTCRtpSynchronizationSource>();
            var jsObjectRefGetSynchronizationSources =
                JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getSynchronizationSources");
            var jsObjectRefRtpSynchronizationSourceArray = 
                JsRuntime.GetJsPropertyArray(jsObjectRefGetSynchronizationSources);
            foreach (var jsObjectRefRtpSynchronizationSource in jsObjectRefRtpSynchronizationSourceArray)
            {
                rtpSynchronizationSources.Add(JsRuntime.GetJsPropertyValue<RTCRtpSynchronizationSource>(
                    jsObjectRefRtpSynchronizationSource, null));
                JsRuntime.DeleteJsObjectRef(jsObjectRefRtpSynchronizationSource.JsObjectRefId);
            }
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetSynchronizationSources.JsObjectRefId);
            return rtpSynchronizationSources.ToArray();
        }

        /*static*/
        public RTCRtpCapabilities GetCapabilities(string kind) =>
            JsRuntime.CallJsMethodWithContent<RTCRtpCapabilities>("RTCRtpReceiver", "getCapabilities", null, kind);
    }
}
