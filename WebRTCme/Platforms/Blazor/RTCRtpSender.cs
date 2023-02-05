using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using System.Linq;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCRtpSender : NativeBase, IRTCRtpSender
    {
        public RTCRtpSender(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCDTMFSender Dtmf =>
            new RTCDTMFSender(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "dtmf"));

        public IMediaStreamTrack Track =>
            new MediaStreamTrack(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "track"));

        public IRTCDtlsTransport Transport =>
            new RTCDtlsTransport(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "transport"));

        public RTCRtpSendParameters GetParameters() =>
            JsRuntime.CallJsMethod<RTCRtpSendParameters>(NativeObject, "getParameters");

        public async Task<IRTCStatsReport> GetStats() =>
            await Task.FromResult(new RTCStatsReport(JsRuntime, await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                NativeObject, "getStats")));

        public Task SetParameters(RTCRtpSendParameters parameters) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "setParameters", parameters).AsTask();

        public void SetStreams(IMediaStream[] mediaStreams) =>
            JsRuntime.CallJsMethodVoid(
                NativeObject, "setStreams", mediaStreams.Select(stream => ((MediaStream)stream).NativeObject).ToArray());

        public Task ReplaceTrack(IMediaStreamTrack newTrack = null) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "replaceTrack", ((MediaStreamTrack)newTrack)?.NativeObject).AsTask();

        /*static*/
        public RTCRtpCapabilities GetCapabilities(string kind) =>
            JsRuntime.CallJsMethod<RTCRtpCapabilities>("RTCRtpSender", "getCapabilities", kind);
    }
}
