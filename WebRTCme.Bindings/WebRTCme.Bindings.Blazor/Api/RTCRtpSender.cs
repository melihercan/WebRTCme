using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using System.Linq;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class RTCRtpSender : ApiBase, IRTCRtpSender
    {
        public static IRTCRtpSender Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtpSender) => 
            new RTCRtpSender(jsRuntime, jsObjectRefRtpSender);

        private RTCRtpSender(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCDTMFSender Dtmf =>
            RTCDTMFSender.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "dtmf"));

        public IMediaStreamTrack Track =>
            MediaStreamTrack.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "track"));

        public IRTCDtlsTransport Transport =>
            RTCDtlsTransport.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "transport"));

        public RTCRtpSendParameters GetParameters() =>
            JsRuntime.CallJsMethod<RTCRtpSendParameters>(NativeObject, "getParameters");

        public async Task<IRTCStatsReport> GetStats() =>
            await Task.FromResult(RTCStatsReport.Create(JsRuntime, await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                NativeObject, "getStats")));

        public Task SetParameters(RTCRtpSendParameters parameters) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "setParameters", parameters).AsTask();

        public void SetStreams(IMediaStream[] mediaStreams) =>
            JsRuntime.CallJsMethodVoid(
                NativeObject, "setStreams", mediaStreams.Select(stream => stream.NativeObject).ToArray());

        public Task ReplaceTrack(IMediaStreamTrack newTrack = null) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "replaceTrack", newTrack?.NativeObject).AsTask();

        /*static*/
        public RTCRtpCapabilities GetCapabilities(string kind) =>
            JsRuntime.CallJsMethod<RTCRtpCapabilities>("RTCRtpSender", "getCapabilities", kind);
    }
}
