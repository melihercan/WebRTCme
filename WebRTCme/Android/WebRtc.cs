using Android.Content;
using Android.Util;
using Microsoft.JSInterop;
using Org.Webrtc.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtc.Android;
using Xamarin.Essentials;
using Webrtc = Org.Webrtc;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        //private static Webrtc.PeerConnectionFactory _nativePeerConnectionFactory;

        public static Webrtc.PeerConnectionFactory NativePeerConnectionFactory { get; private set; }// =>
          //  _nativePeerConnectionFactory ?? (_nativePeerConnectionFactory = 
            //    Webrtc.PeerConnectionFactory.InvokeBuilder(Platform.CurrentActivity.Application)
              //      .SetVideoEncoderFactory(new Webrtc.DefaultVideoEncoderFactory()

        public static IWebRtc Create() => new WebRtc();

        private WebRtc()
        {
            var context = Platform.CurrentActivity.ApplicationContext;

            var options = Webrtc.PeerConnectionFactory.InitializationOptions
                    .InvokeBuilder(context)
                    //.SetFieldTrials("")
                    .SetEnableInternalTracer(true);
            Webrtc.PeerConnectionFactory.Initialize(options.CreateInitializationOptions());


            //var eglBaseContext = EglBaseHelper.Create().EglBaseContext;
            var adm = CreateJavaAudioDevice(context);
            //var encoderFactory = new Webrtc.DefaultVideoEncoderFactory(eglBaseContext, true, true);
            var encoderFactory = new Webrtc.SoftwareVideoEncoderFactory();
            //var decoderFactory = new Webrtc.DefaultVideoDecoderFactory(eglBaseContext);
            var decoderFactory = new Webrtc.SoftwareVideoDecoderFactory();
            var factory = Webrtc.PeerConnectionFactory.InvokeBuilder()
                .SetAudioDeviceModule(adm)
                .SetVideoEncoderFactory(encoderFactory)
                .SetVideoDecoderFactory(decoderFactory)
                .CreatePeerConnectionFactory();
            adm.Release();
            NativePeerConnectionFactory = factory;




        }

        public void Cleanup() 
        { 
        }

        public IWindow Window(IJSRuntime jsRuntime) => global::WebRtc.Android.Window.Create();

        private static IAudioDeviceModule CreateJavaAudioDevice(Context context)
        {
            var audioErrorCallbacks = new AudioErrorCallbacks();
            return JavaAudioDeviceModule.InvokeBuilder(context)
                .SetAudioRecordErrorCallback(audioErrorCallbacks)
                .SetAudioRecordStateCallback(audioErrorCallbacks)
                .SetAudioTrackErrorCallback(audioErrorCallbacks)
                .SetAudioTrackStateCallback(audioErrorCallbacks)
                .CreateAudioDeviceModule();
        }

        private class AudioErrorCallbacks : Java.Lang.Object,
            JavaAudioDeviceModule.IAudioRecordErrorCallback,
            JavaAudioDeviceModule.IAudioRecordStateCallback,
            JavaAudioDeviceModule.IAudioTrackErrorCallback,
            JavaAudioDeviceModule.IAudioTrackStateCallback

        {
            private const string TAG = nameof(NativePeerConnectionFactory);

            public void OnWebRtcAudioRecordError(string p0)
            {
                Log.Error(TAG, $"OnWebRtcAudioRecordError: {p0}");
            }

            public void OnWebRtcAudioRecordInitError(string p0)
            {
                Log.Error(TAG, $"OnWebRtcAudioRecordInitError: {p0}");
            }

            public void OnWebRtcAudioRecordStartError(JavaAudioDeviceModule.AudioRecordStartErrorCode p0, string p1)
            {
                Log.Error(TAG, $"OnWebRtcAudioRecordStartError: errorCode {p0} {p1}");
            }

            public void OnWebRtcAudioTrackError(string p0)
            {
                Log.Error(TAG, $"OnWebRtcAudioTrackError: errorCode {p0}");
            }

            public void OnWebRtcAudioTrackInitError(string p0)
            {
                Log.Error(TAG, $"OnWebRtcAudioTrackInitError: errorCode {p0}");
            }

            public void OnWebRtcAudioTrackStartError(JavaAudioDeviceModule.AudioTrackStartErrorCode p0, string p1)
            {
                Log.Error(TAG, $"OnWebRtcAudioTrackStartError: errorCode {p0} {p1}");
            }

            public void OnWebRtcAudioRecordStart()
            {
                Log.Info(TAG, "Audio recording starts");
            }

            public void OnWebRtcAudioRecordStop()
            {
                Log.Info(TAG, "Audio recording stops");
            }

            public void OnWebRtcAudioTrackStart()
            {
                Log.Info(TAG, "Audio playout starts");
            }

            public void OnWebRtcAudioTrackStop()
            {
                Log.Info(TAG, "Audio playout stops");
            }
        }



    }
}
