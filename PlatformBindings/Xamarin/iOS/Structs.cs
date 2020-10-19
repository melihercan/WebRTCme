using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
//using WebRTC;

namespace WebRtc
{
	[Native]
	public enum RTCVideoRotation : long////nint
	{
		RTCVideoRotation_0 = 0,
		RTCVideoRotation_90 = 90,
		RTCVideoRotation_180 = 180,
		RTCVideoRotation_270 = 270
	}

	[Native]
	public enum RTCFrameType : ulong////nuint
	{
		EmptyFrame = 0,
		AudioFrameSpeech = 1,
		AudioFrameCN = 2,
		VideoFrameKey = 3,
		VideoFrameDelta = 4
	}

	[Native]
	public enum RTCVideoContentType : ulong////nuint
	{
		Unspecified,
		Screenshare
	}

	[Native]
	public enum RTCLoggingSeverity : long////nint
	{
		Verbose,
		Info,
		Warning,
		Error,
		None
	}

	static class CFunctions
	{
		// extern void RTCLogEx (RTCLoggingSeverity severity, NSString *log_string) __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern void RTCLogEx(RTCLoggingSeverity severity, NSString log_string);

		// extern void RTCSetMinDebugLogLevel (RTCLoggingSeverity severity) __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern void RTCSetMinDebugLogLevel(RTCLoggingSeverity severity);

		// extern NSString * RTCFileName (const char *filePath) __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern unsafe NSString RTCFileName(sbyte* filePath);

		// extern void RTCInitFieldTrialDictionary (NSDictionary<NSString *,NSString *> *fieldTrials) __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern void RTCInitFieldTrialDictionary(NSDictionary<NSString, NSString> fieldTrials);

		// extern void RTCEnableMetrics () __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern void RTCEnableMetrics();

		// extern NSArray<RTCMetricsSampleInfo *> * RTCGetAndResetMetrics () __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern RTCMetricsSampleInfo[] RTCGetAndResetMetrics();

		// extern BOOL RTCInitializeSSL () __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern bool RTCInitializeSSL();

		// extern BOOL RTCCleanupSSL () __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern bool RTCCleanupSSL();

		// extern void RTCSetupInternalTracer () __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern void RTCSetupInternalTracer();

		// extern BOOL RTCStartInternalCapture (NSString *filePath) __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern bool RTCStartInternalCapture(NSString filePath);

		// extern void RTCStopInternalCapture () __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern void RTCStopInternalCapture();

		// extern void RTCShutdownInternalTracer () __attribute__((visibility("default")));
		[DllImport("__Internal")]
		////[Verify(PlatformInvoke)]
		static extern void RTCShutdownInternalTracer();
	}

	[Native]
	public enum RTCVideoCodecMode : ulong////nuint
	{
		RealtimeVideo,
		Screensharing
	}

	[Native]
	public enum RTCH264PacketizationMode : ulong////nuint
	{
		NonInterleaved = 0,
		SingleNalUnit
	}

	[Native]
	public enum RTCH264Profile : ulong////nuint
	{
		ConstrainedBaseline,
		Baseline,
		Main,
		ConstrainedHigh,
		High
	}

	[Native]
	public enum RTCH264Level : ulong////nuint
	{
		RTCH264Level1_b = 0,
		RTCH264Level1 = 10,
		RTCH264Level1_1 = 11,
		RTCH264Level1_2 = 12,
		RTCH264Level1_3 = 13,
		RTCH264Level2 = 20,
		RTCH264Level2_1 = 21,
		RTCH264Level2_2 = 22,
		RTCH264Level3 = 30,
		RTCH264Level3_1 = 31,
		RTCH264Level3_2 = 32,
		RTCH264Level4 = 40,
		RTCH264Level4_1 = 41,
		RTCH264Level4_2 = 42,
		RTCH264Level5 = 50,
		RTCH264Level5_1 = 51,
		RTCH264Level5_2 = 52
	}

	[Native]
	public enum RTCDispatcherQueueType : long////nint
	{
		Main,
		CaptureSession,
		AudioSession,
		NetworkMonitor
	}

	[Native]
	public enum RTCDeviceType : long////nint
	{
		Unknown,
		IPhone1G,
		IPhone3G,
		IPhone3GS,
		IPhone4,
		IPhone4Verizon,
		IPhone4S,
		IPhone5GSM,
		IPhone5GSM_CDMA,
		IPhone5CGSM,
		IPhone5CGSM_CDMA,
		IPhone5SGSM,
		IPhone5SGSM_CDMA,
		IPhone6Plus,
		IPhone6,
		IPhone6S,
		IPhone6SPlus,
		IPhone7,
		IPhone7Plus,
		IPhoneSE,
		IPhone8,
		IPhone8Plus,
		IPhoneX,
		IPhoneXS,
		IPhoneXSMax,
		IPhoneXR,
		IPhone11,
		IPhone11Pro,
		IPhone11ProMax,
		IPodTouch1G,
		IPodTouch2G,
		IPodTouch3G,
		IPodTouch4G,
		IPodTouch5G,
		IPodTouch6G,
		IPodTouch7G,
		IPad,
		IPad2Wifi,
		IPad2GSM,
		IPad2CDMA,
		IPad2Wifi2,
		IPadMiniWifi,
		IPadMiniGSM,
		IPadMiniGSM_CDMA,
		IPad3Wifi,
		IPad3GSM_CDMA,
		IPad3GSM,
		IPad4Wifi,
		IPad4GSM,
		IPad4GSM_CDMA,
		IPad5,
		IPad6,
		IPadAirWifi,
		IPadAirCellular,
		IPadAirWifiCellular,
		IPadAir2,
		IPadMini2GWifi,
		IPadMini2GCellular,
		IPadMini2GWifiCellular,
		IPadMini3,
		IPadMini4,
		IPadPro9Inch,
		IPadPro12Inch,
		IPadPro12Inch2,
		IPadPro10Inch,
		IPad7Gen10Inch,
		IPadPro3Gen11Inch,
		IPadPro3Gen12Inch,
		IPadMini5Gen,
		IPadAir3Gen,
		Simulatori386,
		Simulatorx86_64
	}

	[Native]
	public enum RTCSourceState : long////nint
	{
		Initializing,
		Live,
		Ended,
		Muted
	}

	[Native]
	public enum RTCMediaStreamTrackState : long////nint
	{
		Live,
		Ended
	}

	[Native]
	public enum RTCIceTransportPolicy : long////nint
	{
		None,
		Relay,
		NoHost,
		All
	}

	[Native]
	public enum RTCBundlePolicy : long////nint
	{
		Balanced,
		MaxCompat,
		MaxBundle
	}

	[Native]
	public enum RTCRtcpMuxPolicy : long////nint
	{
		Negotiate,
		Require
	}

	[Native]
	public enum RTCTcpCandidatePolicy : long////nint
	{
		Enabled,
		Disabled
	}

	[Native]
	public enum RTCCandidateNetworkPolicy : long////nint
	{
		All,
		LowCost
	}

	[Native]
	public enum RTCContinualGatheringPolicy : long////nint
	{
		Once,
		Continually
	}

	[Native]
	public enum RTCEncryptionKeyType : long////nint
	{
		Rsa,
		Ecdsa
	}

	[Native]
	public enum RTCSdpSemantics : long////nint
	{
		PlanB,
		UnifiedPlan
	}

	[Native]
	public enum RTCDataChannelState : long////nint
	{
		Connecting,
		Open,
		Closing,
		Closed
	}

	[Native]
	public enum RTCTlsCertPolicy : ulong////nuint
	{
		Secure,
		InsecureNoCheck
	}

	[Native]
	public enum RTCSignalingState : long////nint
	{
		Stable,
		HaveLocalOffer,
		HaveLocalPrAnswer,
		HaveRemoteOffer,
		HaveRemotePrAnswer,
		Closed
	}

	[Native]
	public enum RTCIceConnectionState : long////nint
	{
		New,
		Checking,
		Connected,
		Completed,
		Failed,
		Disconnected,
		Closed,
		Count
	}

	[Native]
	public enum RTCPeerConnectionState : long////nint
	{
		New,
		Connecting,
		Connected,
		Disconnected,
		Failed,
		Closed
	}

	[Native]
	public enum RTCIceGatheringState : long////nint
	{
		New,
		Gathering,
		Complete
	}

	[Native]
	public enum RTCStatsOutputLevel : long////nint
	{
		Standard,
		Debug
	}

	[Native]
	public enum RTCPriority : long////nint
	{
		VeryLow,
		Low,
		Medium,
		High
	}

	[Native]
	public enum RTCDegradationPreference : long////nint
	{
		Disabled,
		MaintainFramerate,
		MaintainResolution,
		Balanced
	}

	[Native]
	public enum RTCRtpMediaType : long////nint
	{
		Audio,
		Video,
		Data
	}

	[Native]
	public enum RTCRtpTransceiverDirection : long////nint
	{
		SendRecv,
		SendOnly,
		RecvOnly,
		Inactive,
		Stopped
	}

	[Native]
	public enum RTCSdpType : long////nint
	{
		Offer,
		PrAnswer,
		Answer
	}

	[Native]
	public enum RTCFileLoggerSeverity : ulong////nuint
	{
		Verbose,
		Info,
		Warning,
		Error
	}

	[Native]
	public enum RTCFileLoggerRotationType : ulong////nuint
	{
		Call,
		App
	}

}
