using System;
using AVFoundation;
using AudioToolbox;
using AudioUnit;
////using CoreAudioTypes;
using CoreGraphics;
using CoreVideo;
using Foundation;
using ObjCRuntime;
using UIKit;
////using WebRTC;

namespace Webrtc
{
    //// WebRTCme additions.
    delegate void dispatch_block_t();

    interface IRTCVideoFrameBuffer 
	{
        [Abstract]
        [Export("width")]
        int Width { get; }

        // @required @property (readonly, nonatomic) int height;
        [Abstract]
        [Export("height")]
        int Height { get; }

        // @required -(id<RTCI420Buffer> _Nonnull)toI420;
        [Abstract]
        [Export("toI420")]
        ////[Verify (MethodToProperty)]
        RTCI420Buffer ToI420 { get; }

    }
    interface IRTCMutableI420Buffer { }
	interface IRTCVideoRenderer 
	{
        [Abstract]
        [Export("setSize:")]
        void SetSize(CGSize size);

        // @required -(void)renderFrame:(RTCVideoFrame * _Nullable)frame;
        [Abstract]
        [Export("renderFrame:")]
        void RenderFrame([NullAllowed] RTCVideoFrame frame);
    }
    interface IRTCYUVPlanarBuffer : IRTCVideoFrameBuffer
	{
        [Abstract]
        [Export("chromaWidth")]
        int ChromaWidth { get; }

        // @required @property (readonly, nonatomic) int chromaHeight;
        [Abstract]
        [Export("chromaHeight")]
        int ChromaHeight { get; }

        // @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataY;
        [Abstract]
        [Export("dataY")]
        /****unsafe byte* ****/
        IntPtr DataY { get; }

        // @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataU;
        [Abstract]
        [Export("dataU")]
        /****unsafebyte* ****/
        IntPtr DataU { get; }

        // @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataV;
        [Abstract]
        [Export("dataV")]
        /****unsafebyte* ****/
        IntPtr DataV { get; }

        // @required @property (readonly, nonatomic) int strideY;
        [Abstract]
        [Export("strideY")]
        int StrideY { get; }

        // @required @property (readonly, nonatomic) int strideU;
        [Abstract]
        [Export("strideU")]
        int StrideU { get; }

        // @required @property (readonly, nonatomic) int strideV;
        [Abstract]
        [Export("strideV")]
        int StrideV { get; }

        // @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height dataY:(const uint8_t * _Nonnull)dataY dataU:(const uint8_t * _Nonnull)dataU dataV:(const uint8_t * _Nonnull)dataV;
        [Abstract]
        [Export("initWithWidth:height:dataY:dataU:dataV:")]
        /****unsafe****/
        NativeHandle /****Height****/InitWithWidth(int width, int height, /****byte* ****/IntPtr dataY, /****byte* ****/IntPtr dataU, /****byte* ***/IntPtr dataV);

        // @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height;
        [Abstract]
        [Export("initWithWidth:height:")]
        NativeHandle /****Height****/InitWithWidth(int width, int height);

        // @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height strideY:(int)strideY strideU:(int)strideU strideV:(int)strideV;
        [Abstract]
        [Export("initWithWidth:height:strideY:strideU:strideV:")]
        NativeHandle /****Height****/InitWithWidth(int width, int height, int strideY, int strideU, int strideV);

    }
    interface IRTCVideoDecoderFactory { }
    interface IRTCVideoEncoderFactory { }
	interface IRTCVideoEncoder { }
	interface IRTCI420Buffer { }
	interface IRTCAudioSessionActivationDelegate { }
    interface IRTCVideoDecoder { }
    interface IRTCCodecSpecificInfo { }
	interface IRTCVideoCapturerDelegate 
	{
        [Abstract]
        [Export("capturer:didCaptureVideoFrame:")]
        void DidCaptureVideoFrame(RTCVideoCapturer capturer, RTCVideoFrame frame);

    }
    interface IRTCDataChannelDelegate { }
	interface IRTCPeerConnectionDelegate { }
	interface IRTCVideoViewDelegate { }
    interface IRTCRtpReceiverDelegate { }

    // @protocol RTCCodecSpecificInfo <NSObject>
    /*
    Check whether adding [Model] to this declaration is appropriate.
    [Model] is used to generate a C# class that implements this protocol,
    and might be useful for protocols that consumers are supposed to implement,
    since consumers can subclass the generated class instead of implementing
    the generated interface. If consumers are not supposed to implement this
    protocol, then [Model] is redundant and will generate code that will never
    be used.
    */
    [Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCCodecSpecificInfo
	{
	}

	// @interface RTCVideoFrame : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCVideoFrame
	{
		// @property (readonly, nonatomic) int width;
		[Export ("width")]
		int Width { get; }

		// @property (readonly, nonatomic) int height;
		[Export ("height")]
		int Height { get; }

		// @property (readonly, nonatomic) RTCVideoRotation rotation;
		[Export ("rotation")]
		RTCVideoRotation Rotation { get; }

		// @property (readonly, nonatomic) int64_t timeStampNs;
		[Export ("timeStampNs")]
		long TimeStampNs { get; }

		// @property (assign, nonatomic) int32_t timeStamp;
		[Export ("timeStamp")]
		int TimeStamp { get; set; }

		// @property (readonly, nonatomic) id<RTCVideoFrameBuffer> _Nonnull buffer;
		[Export ("buffer")]
		RTCVideoFrameBuffer Buffer { get; }

		// -(instancetype _Nonnull)initWithPixelBuffer:(CVPixelBufferRef _Nonnull)pixelBuffer rotation:(RTCVideoRotation)rotation timeStampNs:(int64_t)timeStampNs __attribute__((deprecated("use initWithBuffer instead")));
		[Export ("initWithPixelBuffer:rotation:timeStampNs:")]
		NativeHandle Constructor (CVPixelBuffer pixelBuffer, RTCVideoRotation rotation, long timeStampNs);

		// -(instancetype _Nonnull)initWithPixelBuffer:(CVPixelBufferRef _Nonnull)pixelBuffer scaledWidth:(int)scaledWidth scaledHeight:(int)scaledHeight cropWidth:(int)cropWidth cropHeight:(int)cropHeight cropX:(int)cropX cropY:(int)cropY rotation:(RTCVideoRotation)rotation timeStampNs:(int64_t)timeStampNs __attribute__((deprecated("use initWithBuffer instead")));
		[Export ("initWithPixelBuffer:scaledWidth:scaledHeight:cropWidth:cropHeight:cropX:cropY:rotation:timeStampNs:")]
		NativeHandle Constructor (CVPixelBuffer pixelBuffer, int scaledWidth, int scaledHeight, int cropWidth, int cropHeight, int cropX, int cropY, RTCVideoRotation rotation, long timeStampNs);

		// -(instancetype _Nonnull)initWithBuffer:(id<RTCVideoFrameBuffer> _Nonnull)frameBuffer rotation:(RTCVideoRotation)rotation timeStampNs:(int64_t)timeStampNs;
		[Export ("initWithBuffer:rotation:timeStampNs:")]
		NativeHandle Constructor (RTCVideoFrameBuffer frameBuffer, RTCVideoRotation rotation, long timeStampNs);

		// -(RTCVideoFrame * _Nonnull)newI420VideoFrame;
		[Export ("newI420VideoFrame")]
		////[Verify (MethodToProperty)]
		RTCVideoFrame NewI420VideoFrame { get; }
	}

	// @interface RTCEncodedImage : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCEncodedImage
	{
		// @property (nonatomic, strong) NSData * _Nonnull buffer;
		[Export ("buffer", ArgumentSemantic.Strong)]
		NSData Buffer { get; set; }

		// @property (assign, nonatomic) int32_t encodedWidth;
		[Export ("encodedWidth")]
		int EncodedWidth { get; set; }

		// @property (assign, nonatomic) int32_t encodedHeight;
		[Export ("encodedHeight")]
		int EncodedHeight { get; set; }

		// @property (assign, nonatomic) uint32_t timeStamp;
		[Export ("timeStamp")]
		uint TimeStamp { get; set; }

		// @property (assign, nonatomic) int64_t captureTimeMs;
		[Export ("captureTimeMs")]
		long CaptureTimeMs { get; set; }

		// @property (assign, nonatomic) int64_t ntpTimeMs;
		[Export ("ntpTimeMs")]
		long NtpTimeMs { get; set; }

		// @property (assign, nonatomic) uint8_t flags;
		[Export ("flags")]
		byte Flags { get; set; }

		// @property (assign, nonatomic) int64_t encodeStartMs;
		[Export ("encodeStartMs")]
		long EncodeStartMs { get; set; }

		// @property (assign, nonatomic) int64_t encodeFinishMs;
		[Export ("encodeFinishMs")]
		long EncodeFinishMs { get; set; }

		// @property (assign, nonatomic) RTCFrameType frameType;
		[Export ("frameType", ArgumentSemantic.Assign)]
		RTCFrameType FrameType { get; set; }

		// @property (assign, nonatomic) RTCVideoRotation rotation;
		[Export ("rotation", ArgumentSemantic.Assign)]
		RTCVideoRotation Rotation { get; set; }

		// @property (nonatomic, strong) NSNumber * _Nonnull qp;
		[Export ("qp", ArgumentSemantic.Strong)]
		NSNumber Qp { get; set; }

		// @property (assign, nonatomic) RTCVideoContentType contentType;
		[Export ("contentType", ArgumentSemantic.Assign)]
		RTCVideoContentType ContentType { get; set; }
	}

	// @protocol RTCVideoFrameBuffer <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCVideoFrameBuffer
	{
		// @required @property (readonly, nonatomic) int width;
		[Abstract]
		[Export ("width")]
		int Width { get; }

		// @required @property (readonly, nonatomic) int height;
		[Abstract]
		[Export ("height")]
		int Height { get; }

		// @required -(id<RTCI420Buffer> _Nonnull)toI420;
		[Abstract]
		[Export ("toI420")]
		////[Verify (MethodToProperty)]
		RTCI420Buffer ToI420 { get; }

		// @optional -(id<RTCVideoFrameBuffer> _Nonnull)cropAndScaleWith:(int)offsetX offsetY:(int)offsetY cropWidth:(int)cropWidth cropHeight:(int)cropHeight scaleWidth:(int)scaleWidth scaleHeight:(int)scaleHeight;
		[Export ("cropAndScaleWith:offsetY:cropWidth:cropHeight:scaleWidth:scaleHeight:")]
		RTCVideoFrameBuffer OffsetY (int offsetX, int offsetY, int cropWidth, int cropHeight, int scaleWidth, int scaleHeight);
	}

	// @protocol RTCYUVPlanarBuffer <RTCVideoFrameBuffer>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	interface RTCYUVPlanarBuffer : /****I****/RTCVideoFrameBuffer
	{
		// @required @property (readonly, nonatomic) int chromaWidth;
		[Abstract]
		[Export ("chromaWidth")]
		int ChromaWidth { get; }

		// @required @property (readonly, nonatomic) int chromaHeight;
		[Abstract]
		[Export ("chromaHeight")]
		int ChromaHeight { get; }

		// @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataY;
		[Abstract]
		[Export ("dataY")]
        /****unsafe byte* ****/IntPtr DataY { get; }

		// @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataU;
		[Abstract]
		[Export ("dataU")]
        /****unsafebyte* ****/IntPtr DataU { get; }

		// @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataV;
		[Abstract]
		[Export ("dataV")]
        /****unsafebyte* ****/IntPtr DataV { get; }

		// @required @property (readonly, nonatomic) int strideY;
		[Abstract]
		[Export ("strideY")]
		int StrideY { get; }

		// @required @property (readonly, nonatomic) int strideU;
		[Abstract]
		[Export ("strideU")]
		int StrideU { get; }

		// @required @property (readonly, nonatomic) int strideV;
		[Abstract]
		[Export ("strideV")]
		int StrideV { get; }

		// @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height dataY:(const uint8_t * _Nonnull)dataY dataU:(const uint8_t * _Nonnull)dataU dataV:(const uint8_t * _Nonnull)dataV;
		[Abstract]
		[Export ("initWithWidth:height:dataY:dataU:dataV:")]
		/****unsafe****/ NativeHandle /****Height****/InitWithWidth(int width, int height, /****byte* ****/IntPtr dataY, /****byte* ****/IntPtr dataU, /****byte* ***/IntPtr dataV);

		// @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height;
		[Abstract]
		[Export ("initWithWidth:height:")]
		NativeHandle /****Height****/InitWithWidth(int width, int height);

		// @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height strideY:(int)strideY strideU:(int)strideU strideV:(int)strideV;
		[Abstract]
		[Export ("initWithWidth:height:strideY:strideU:strideV:")]
		NativeHandle /****Height****/InitWithWidth(int width, int height, int strideY, int strideU, int strideV);
	}

	// @protocol RTCI420Buffer <RTCYUVPlanarBuffer>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/////[Protocol]
	////interface RTCI420Buffer : IRTCYUVPlanarBuffer
	////{
	////}

	// @protocol RTCMutableYUVPlanarBuffer <RTCYUVPlanarBuffer>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	interface RTCMutableYUVPlanarBuffer : /****I****/RTCYUVPlanarBuffer
	{
		// @required @property (readonly, nonatomic) uint8_t * _Nonnull mutableDataY;
		[Abstract]
		[Export ("mutableDataY")]
		/****unsafe byte* ****/IntPtr MutableDataY { get; }

		// @required @property (readonly, nonatomic) uint8_t * _Nonnull mutableDataU;
		[Abstract]
		[Export ("mutableDataU")]
		/****unsafe byte* ****/IntPtr MutableDataU { get; }

		// @required @property (readonly, nonatomic) uint8_t * _Nonnull mutableDataV;
		[Abstract]
		[Export ("mutableDataV")]
		/**** unsafe byte* ****/IntPtr MutableDataV { get; }
	}

    // @protocol RTCMutableI420Buffer <RTCI420Buffer, RTCMutableYUVPlanarBuffer>
    /*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/////[Protocol]
  ////interface RTCMutableI420Buffer : IRTCI420Buffer, IRTCMutableYUVPlanarBuffer
  ////{
  ////}

    // @protocol RTCSSLCertificateVerifier <NSObject>
    /*
    Check whether adding [Model] to this declaration is appropriate.
    [Model] is used to generate a C# class that implements this protocol,
    and might be useful for protocols that consumers are supposed to implement,
    since consumers can subclass the generated class instead of implementing
    the generated interface. If consumers are not supposed to implement this
    protocol, then [Model] is redundant and will generate code that will never
    be used.
    */
    [Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCSSLCertificateVerifier
	{
		// @required -(BOOL)verify:(NSData * _Nonnull)derCertificate;
		[Abstract]
		[Export ("verify:")]
		bool Verify (NSData derCertificate);
	}

	// @protocol RTCVideoCapturerDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface RTCVideoCapturerDelegate
	{
		// @required -(void)capturer:(RTCVideoCapturer * _Nonnull)capturer didCaptureVideoFrame:(RTCVideoFrame * _Nonnull)frame;
		[Abstract]
		[Export ("capturer:didCaptureVideoFrame:")]
		void DidCaptureVideoFrame (RTCVideoCapturer capturer, RTCVideoFrame frame);
	}

	// @interface RTCVideoCapturer : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoCapturer
	{
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		IRTCVideoCapturerDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<RTCVideoCapturerDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// -(instancetype _Nonnull)initWithDelegate:(id<RTCVideoCapturerDelegate> _Nonnull)delegate;
		[Export ("initWithDelegate:")]
		NativeHandle Constructor (RTCVideoCapturerDelegate @delegate);
	}

	// @interface RTCVideoCodecInfo : NSObject <NSCoding>
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCVideoCodecInfo : INSCoding
	{
		// -(instancetype _Nonnull)initWithName:(NSString * _Nonnull)name;
		[Export ("initWithName:")]
		NativeHandle Constructor (string name);

		// -(instancetype _Nonnull)initWithName:(NSString * _Nonnull)name parameters:(NSDictionary<NSString *,NSString *> * _Nullable)parameters __attribute__((objc_designated_initializer));
		[Export ("initWithName:parameters:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string name, [NullAllowed] NSDictionary<NSString, NSString> parameters);

		// -(BOOL)isEqualToCodecInfo:(RTCVideoCodecInfo * _Nonnull)info;
		[Export ("isEqualToCodecInfo:")]
		bool IsEqualToCodecInfo (RTCVideoCodecInfo info);

		// @property (readonly, nonatomic) NSString * _Nonnull name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, nonatomic) NSDictionary<NSString *,NSString *> * _Nonnull parameters;
		[Export ("parameters")]
		NSDictionary<NSString, NSString> Parameters { get; }
	}

	// @interface RTCVideoEncoderSettings : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderSettings
	{
		// @property (nonatomic, strong) NSString * _Nonnull name;
		[Export ("name", ArgumentSemantic.Strong)]
		string Name { get; set; }

		// @property (assign, nonatomic) unsigned short width;
		[Export ("width")]
		ushort Width { get; set; }

		// @property (assign, nonatomic) unsigned short height;
		[Export ("height")]
		ushort Height { get; set; }

		// @property (assign, nonatomic) unsigned int startBitrate;
		[Export ("startBitrate")]
		uint StartBitrate { get; set; }

		// @property (assign, nonatomic) unsigned int maxBitrate;
		[Export ("maxBitrate")]
		uint MaxBitrate { get; set; }

		// @property (assign, nonatomic) unsigned int minBitrate;
		[Export ("minBitrate")]
		uint MinBitrate { get; set; }

		// @property (assign, nonatomic) uint32_t maxFramerate;
		[Export ("maxFramerate")]
		uint MaxFramerate { get; set; }

		// @property (assign, nonatomic) unsigned int qpMax;
		[Export ("qpMax")]
		uint QpMax { get; set; }

		// @property (assign, nonatomic) RTCVideoCodecMode mode;
		[Export ("mode", ArgumentSemantic.Assign)]
		RTCVideoCodecMode Mode { get; set; }
	}

	// typedef void (^RTCVideoDecoderCallback)(RTCVideoFrame * _Nonnull);
	delegate void RTCVideoDecoderCallback (RTCVideoFrame arg0);

	// @protocol RTCVideoDecoder <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCVideoDecoder
	{
		// @required -(void)setCallback:(RTCVideoDecoderCallback _Nonnull)callback;
		[Abstract]
		[Export ("setCallback:")]
		void SetCallback (RTCVideoDecoderCallback callback);

		// @required -(NSInteger)startDecodeWithNumberOfCores:(int)numberOfCores;
		[Abstract]
		[Export ("startDecodeWithNumberOfCores:")]
		nint StartDecodeWithNumberOfCores (int numberOfCores);

		// @required -(NSInteger)releaseDecoder;
		[Abstract]
		[Export ("releaseDecoder")]
		////[Verify (MethodToProperty)]
		nint ReleaseDecoder { get; }

		// @required -(NSInteger)decode:(RTCEncodedImage * _Nonnull)encodedImage missingFrames:(BOOL)missingFrames codecSpecificInfo:(id<RTCCodecSpecificInfo> _Nullable)info renderTimeMs:(int64_t)renderTimeMs;
		[Abstract]
		[Export ("decode:missingFrames:codecSpecificInfo:renderTimeMs:")]
		nint Decode (RTCEncodedImage encodedImage, bool missingFrames, [NullAllowed] RTCCodecSpecificInfo info, long renderTimeMs);

		// @required -(NSString * _Nonnull)implementationName;
		[Abstract]
		[Export ("implementationName")]
		////[Verify (MethodToProperty)]
		string ImplementationName { get; }
	}

	// @protocol RTCVideoDecoderFactory <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCVideoDecoderFactory
	{
		// @required -(id<RTCVideoDecoder> _Nullable)createDecoder:(RTCVideoCodecInfo * _Nonnull)info;
		[Abstract]
		[Export ("createDecoder:")]
		[return: NullAllowed]
		RTCVideoDecoder CreateDecoder (RTCVideoCodecInfo info);

		// @required -(NSArray<RTCVideoCodecInfo *> * _Nonnull)supportedCodecs;
		[Abstract]
		[Export ("supportedCodecs")]
		////[Verify (MethodToProperty)]
		RTCVideoCodecInfo[] SupportedCodecs { get; }
	}

	// @interface RTCVideoEncoderQpThresholds : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderQpThresholds
	{
		// -(instancetype _Nonnull)initWithThresholdsLow:(NSInteger)low high:(NSInteger)high;
		[Export ("initWithThresholdsLow:high:")]
		NativeHandle Constructor (nint low, nint high);

		// @property (readonly, nonatomic) NSInteger low;
		[Export ("low")]
		nint Low { get; }

		// @property (readonly, nonatomic) NSInteger high;
		[Export ("high")]
		nint High { get; }
	}

	// typedef BOOL (^RTCVideoEncoderCallback)(RTCEncodedImage * _Nonnull, id<RTCCodecSpecificInfo> _Nonnull);
	delegate bool RTCVideoEncoderCallback (RTCEncodedImage arg0, RTCCodecSpecificInfo arg1);

	// @protocol RTCVideoEncoder <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoder
	{
		// @required -(void)setCallback:(RTCVideoEncoderCallback _Nullable)callback;
		[Abstract]
		[Export ("setCallback:")]
		void SetCallback ([NullAllowed] RTCVideoEncoderCallback callback);

		// @required -(NSInteger)startEncodeWithSettings:(RTCVideoEncoderSettings * _Nonnull)settings numberOfCores:(int)numberOfCores;
		[Abstract]
		[Export ("startEncodeWithSettings:numberOfCores:")]
		nint StartEncodeWithSettings (RTCVideoEncoderSettings settings, int numberOfCores);

		// @required -(NSInteger)releaseEncoder;
		[Abstract]
		[Export ("releaseEncoder")]
		////[Verify (MethodToProperty)]
		nint ReleaseEncoder { get; }

		// @required -(NSInteger)encode:(RTCVideoFrame * _Nonnull)frame codecSpecificInfo:(id<RTCCodecSpecificInfo> _Nullable)info frameTypes:(NSArray<NSNumber *> * _Nonnull)frameTypes;
		[Abstract]
		[Export ("encode:codecSpecificInfo:frameTypes:")]
		nint Encode (RTCVideoFrame frame, [NullAllowed] RTCCodecSpecificInfo info, NSNumber[] frameTypes);

		// @required -(int)setBitrate:(uint32_t)bitrateKbit framerate:(uint32_t)framerate;
		[Abstract]
		[Export ("setBitrate:framerate:")]
		int SetBitrate (uint bitrateKbit, uint framerate);

		// @required -(NSString * _Nonnull)implementationName;
		[Abstract]
		[Export ("implementationName")]
		////[Verify (MethodToProperty)]
		string ImplementationName { get; }

		// @required -(RTCVideoEncoderQpThresholds * _Nullable)scalingSettings;
		[Abstract]
		[NullAllowed, Export ("scalingSettings")]
		////[Verify (MethodToProperty)]
		RTCVideoEncoderQpThresholds ScalingSettings { get; }

		// @required @property (readonly, nonatomic) NSInteger resolutionAlignment;
		[Abstract]
		[Export ("resolutionAlignment")]
		nint ResolutionAlignment { get; }

		// @required @property (readonly, nonatomic) BOOL applyAlignmentToAllSimulcastLayers;
		[Abstract]
		[Export ("applyAlignmentToAllSimulcastLayers")]
		bool ApplyAlignmentToAllSimulcastLayers { get; }

		// @required @property (readonly, nonatomic) BOOL supportsNativeHandle;
		[Abstract]
		[Export ("supportsNativeHandle")]
		bool SupportsNativeHandle { get; }
	}

	// @protocol RTCVideoEncoderSelector <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderSelector
	{
		// @required -(void)registerCurrentEncoderInfo:(RTCVideoCodecInfo * _Nonnull)info;
		[Abstract]
		[Export ("registerCurrentEncoderInfo:")]
		void RegisterCurrentEncoderInfo (RTCVideoCodecInfo info);

		// @required -(RTCVideoCodecInfo * _Nullable)encoderForBitrate:(NSInteger)bitrate;
		[Abstract]
		[Export ("encoderForBitrate:")]
		[return: NullAllowed]
		RTCVideoCodecInfo EncoderForBitrate (nint bitrate);

		// @required -(RTCVideoCodecInfo * _Nullable)encoderForBrokenEncoder;
		[Abstract]
		[NullAllowed, Export ("encoderForBrokenEncoder")]
		////[Verify (MethodToProperty)]
		RTCVideoCodecInfo EncoderForBrokenEncoder { get; }

		// @optional -(RTCVideoCodecInfo * _Nullable)encoderForResolutionChangeBySize:(CGSize)size;
		[Export ("encoderForResolutionChangeBySize:")]
		[return: NullAllowed]
		RTCVideoCodecInfo EncoderForResolutionChangeBySize (CGSize size);
	}

	// @protocol RTCVideoEncoderFactory <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderFactory
	{
		// @required -(id<RTCVideoEncoder> _Nullable)createEncoder:(RTCVideoCodecInfo * _Nonnull)info;
		[Abstract]
		[Export ("createEncoder:")]
		[return: NullAllowed]
		RTCVideoEncoder CreateEncoder (RTCVideoCodecInfo info);

		// @required -(NSArray<RTCVideoCodecInfo *> * _Nonnull)supportedCodecs;
		[Abstract]
		[Export ("supportedCodecs")]
		////[Verify (MethodToProperty)]
		RTCVideoCodecInfo[] SupportedCodecs { get; }

		// @optional -(NSArray<RTCVideoCodecInfo *> * _Nonnull)implementations;
		[Export ("implementations")]
		////[Verify (MethodToProperty)]
		RTCVideoCodecInfo[] Implementations { get; }

		// @optional -(id<RTCVideoEncoderSelector> _Nullable)encoderSelector;
		[NullAllowed, Export ("encoderSelector")]
		////[Verify (MethodToProperty)]
		RTCVideoEncoderSelector EncoderSelector { get; }
	}

	// @protocol RTCVideoRenderer <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCVideoRenderer
	{
		// @required -(void)setSize:(CGSize)size;
		[Abstract]
		[Export ("setSize:")]
		void SetSize (CGSize size);

		// @required -(void)renderFrame:(RTCVideoFrame * _Nullable)frame;
		[Abstract]
		[Export ("renderFrame:")]
		void RenderFrame ([NullAllowed] RTCVideoFrame frame);
	}

	// @protocol RTCVideoViewDelegate
	[Protocol, Model /****(AutoGeneratedName = true)****/]
[BaseType(typeof(NSObject))]
    interface RTCVideoViewDelegate
	{
		// @required -(void)videoView:(id<RTCVideoRenderer> _Nonnull)videoView didChangeVideoSize:(CGSize)size;
		[Abstract]
		[Export ("videoView:didChangeVideoSize:")]
		void DidChangeVideoSize (IRTCVideoRenderer videoView, CGSize size);
	}

	// typedef OSStatus (^RTCAudioDeviceGetPlayoutDataBlock)(AudioUnitRenderActionFlags * _Nonnull, const AudioTimeStamp * _Nonnull, NSInteger, UInt32, AudioBufferList * _Nonnull);
	/****unsafe****/ delegate int RTCAudioDeviceGetPlayoutDataBlock (/****AudioUnitRenderActionFlags* ****/IntPtr arg0, /****AudioTimeStamp* ****/IntPtr arg1, nint arg2, uint arg3, /****AudioBufferList* ****/IntPtr arg4);

	// typedef OSStatus (^RTCAudioDeviceRenderRecordedDataBlock)(AudioUnitRenderActionFlags * _Nonnull, const AudioTimeStamp * _Nonnull, NSInteger, UInt32, AudioBufferList * _Nonnull, void * _Nullable);
	/****unsafe****/ delegate int RTCAudioDeviceRenderRecordedDataBlock (/****AudioUnitRenderActionFlags* ****/IntPtr arg0, /****AudioTimeStamp* ****/IntPtr arg1, nint arg2, uint arg3, /****AudioBufferList* ****/IntPtr arg4, [NullAllowed] /****void* ****/IntPtr arg5);

	// typedef OSStatus (^RTCAudioDeviceDeliverRecordedDataBlock)(AudioUnitRenderActionFlags * _Nonnull, const AudioTimeStamp * _Nonnull, NSInteger, UInt32, const AudioBufferList * _Nullable, void * _Nullable, RTCAudioDeviceRenderRecordedDataBlock _Nullable);
	/****unsafe****/ delegate int RTCAudioDeviceDeliverRecordedDataBlock (/****AudioUnitRenderActionFlags* ****/IntPtr arg0, /****AudioTimeStamp* ***/IntPtr arg1, nint arg2, uint arg3, [NullAllowed] /****AudioBufferList* ****/IntPtr arg4, [NullAllowed] /****void* ****/IntPtr arg5, [NullAllowed] RTCAudioDeviceRenderRecordedDataBlock arg6);

	// @protocol RTCAudioDeviceDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface RTCAudioDeviceDelegate
	{
		// @required @property (readonly) RTCAudioDeviceDeliverRecordedDataBlock _Nonnull deliverRecordedData;
		[Abstract]
		[Export ("deliverRecordedData")]
		RTCAudioDeviceDeliverRecordedDataBlock DeliverRecordedData { get; }

		// @required @property (readonly) double preferredInputSampleRate;
		[Abstract]
		[Export ("preferredInputSampleRate")]
		double PreferredInputSampleRate { get; }

		// @required @property (readonly) NSTimeInterval preferredInputIOBufferDuration;
		[Abstract]
		[Export ("preferredInputIOBufferDuration")]
		double PreferredInputIOBufferDuration { get; }

		// @required @property (readonly) double preferredOutputSampleRate;
		[Abstract]
		[Export ("preferredOutputSampleRate")]
		double PreferredOutputSampleRate { get; }

		// @required @property (readonly) NSTimeInterval preferredOutputIOBufferDuration;
		[Abstract]
		[Export ("preferredOutputIOBufferDuration")]
		double PreferredOutputIOBufferDuration { get; }

		// @required @property (readonly) RTCAudioDeviceGetPlayoutDataBlock _Nonnull getPlayoutData;
		[Abstract]
		[Export ("getPlayoutData")]
		RTCAudioDeviceGetPlayoutDataBlock GetPlayoutData { get; }

		// @required -(void)notifyAudioInputParametersChange;
		[Abstract]
		[Export ("notifyAudioInputParametersChange")]
		void NotifyAudioInputParametersChange ();

		// @required -(void)notifyAudioOutputParametersChange;
		[Abstract]
		[Export ("notifyAudioOutputParametersChange")]
		void NotifyAudioOutputParametersChange ();

		// @required -(void)notifyAudioInputInterrupted;
		[Abstract]
		[Export ("notifyAudioInputInterrupted")]
		void NotifyAudioInputInterrupted ();

		// @required -(void)notifyAudioOutputInterrupted;
		[Abstract]
		[Export ("notifyAudioOutputInterrupted")]
		void NotifyAudioOutputInterrupted ();

		// @required -(void)dispatchAsync:(dispatch_block_t _Nonnull)block;
		[Abstract]
		[Export ("dispatchAsync:")]
		void DispatchAsync (dispatch_block_t block);

		// @required -(void)dispatchSync:(dispatch_block_t _Nonnull)block;
		[Abstract]
		[Export ("dispatchSync:")]
		void DispatchSync (dispatch_block_t block);
	}

	// @protocol RTCAudioDevice <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCAudioDevice
	{
		// @required @property (readonly) double deviceInputSampleRate;
		[Abstract]
		[Export ("deviceInputSampleRate")]
		double DeviceInputSampleRate { get; }

		// @required @property (readonly) NSTimeInterval inputIOBufferDuration;
		[Abstract]
		[Export ("inputIOBufferDuration")]
		double InputIOBufferDuration { get; }

		// @required @property (readonly) NSInteger inputNumberOfChannels;
		[Abstract]
		[Export ("inputNumberOfChannels")]
		nint InputNumberOfChannels { get; }

		// @required @property (readonly) NSTimeInterval inputLatency;
		[Abstract]
		[Export ("inputLatency")]
		double InputLatency { get; }

		// @required @property (readonly) double deviceOutputSampleRate;
		[Abstract]
		[Export ("deviceOutputSampleRate")]
		double DeviceOutputSampleRate { get; }

		// @required @property (readonly) NSTimeInterval outputIOBufferDuration;
		[Abstract]
		[Export ("outputIOBufferDuration")]
		double OutputIOBufferDuration { get; }

		// @required @property (readonly) NSInteger outputNumberOfChannels;
		[Abstract]
		[Export ("outputNumberOfChannels")]
		nint OutputNumberOfChannels { get; }

		// @required @property (readonly) NSTimeInterval outputLatency;
		[Abstract]
		[Export ("outputLatency")]
		double OutputLatency { get; }

		// @required @property (readonly) BOOL isInitialized;
		[Abstract]
		[Export ("isInitialized")]
		bool IsInitialized { get; }

		// @required -(BOOL)initializeWithDelegate:(id<RTCAudioDeviceDelegate> _Nonnull)delegate;
		[Abstract]
		[Export ("initializeWithDelegate:")]
		bool InitializeWithDelegate (RTCAudioDeviceDelegate @delegate);

		// @required -(BOOL)terminateDevice;
		[Abstract]
		[Export ("terminateDevice")]
		////[Verify (MethodToProperty)]
		bool TerminateDevice { get; }

		// @required @property (readonly) BOOL isPlayoutInitialized;
		[Abstract]
		[Export ("isPlayoutInitialized")]
		bool IsPlayoutInitialized { get; }

		// @required -(BOOL)initializePlayout;
		[Abstract]
		[Export ("initializePlayout")]
		////[Verify (MethodToProperty)]
		bool InitializePlayout { get; }

		// @required @property (readonly) BOOL isPlaying;
		[Abstract]
		[Export ("isPlaying")]
		bool IsPlaying { get; }

		// @required -(BOOL)startPlayout;
		[Abstract]
		[Export ("startPlayout")]
		////[Verify (MethodToProperty)]
		bool StartPlayout { get; }

		// @required -(BOOL)stopPlayout;
		[Abstract]
		[Export ("stopPlayout")]
		////[Verify (MethodToProperty)]
		bool StopPlayout { get; }

		// @required @property (readonly) BOOL isRecordingInitialized;
		[Abstract]
		[Export ("isRecordingInitialized")]
		bool IsRecordingInitialized { get; }

		// @required -(BOOL)initializeRecording;
		[Abstract]
		[Export ("initializeRecording")]
		////[Verify (MethodToProperty)]
		bool InitializeRecording { get; }

		// @required @property (readonly) BOOL isRecording;
		[Abstract]
		[Export ("isRecording")]
		bool IsRecording { get; }

		// @required -(BOOL)startRecording;
		[Abstract]
		[Export ("startRecording")]
		////[Verify (MethodToProperty)]
		bool StartRecording { get; }

		// @required -(BOOL)stopRecording;
		[Abstract]
		[Export ("stopRecording")]
		////[Verify (MethodToProperty)]
		bool StopRecording { get; }
	}

	[Static]
	////[Verify (ConstantsInterfaceAssociation)]
	partial interface Constants
	{
		// extern NSString *const _Nonnull kRTCAudioSessionErrorDomain;
		[Field ("kRTCAudioSessionErrorDomain", "__Internal")]
		NSString kRTCAudioSessionErrorDomain { get; }

		// extern const NSInteger kRTCAudioSessionErrorLockRequired;
		[Field ("kRTCAudioSessionErrorLockRequired", "__Internal")]
		nint kRTCAudioSessionErrorLockRequired { get; }

		// extern const NSInteger kRTCAudioSessionErrorConfiguration;
		[Field ("kRTCAudioSessionErrorConfiguration", "__Internal")]
		nint kRTCAudioSessionErrorConfiguration { get; }
	}

	// @protocol RTCAudioSessionDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface RTCAudioSessionDelegate
	{
		// @optional -(void)audioSessionDidBeginInterruption:(RTCAudioSession * _Nonnull)session;
		[Export ("audioSessionDidBeginInterruption:")]
		void AudioSessionDidBeginInterruption (RTCAudioSession session);

		// @optional -(void)audioSessionDidEndInterruption:(RTCAudioSession * _Nonnull)session shouldResumeSession:(BOOL)shouldResumeSession;
		[Export ("audioSessionDidEndInterruption:shouldResumeSession:")]
		void AudioSessionDidEndInterruption (RTCAudioSession session, bool shouldResumeSession);

		// @optional -(void)audioSessionDidChangeRoute:(RTCAudioSession * _Nonnull)session reason:(AVAudioSessionRouteChangeReason)reason previousRoute:(AVAudioSessionRouteDescription * _Nonnull)previousRoute;
		[Export ("audioSessionDidChangeRoute:reason:previousRoute:")]
		void AudioSessionDidChangeRoute (RTCAudioSession session, AVAudioSessionRouteChangeReason reason, AVAudioSessionRouteDescription previousRoute);

		// @optional -(void)audioSessionMediaServerTerminated:(RTCAudioSession * _Nonnull)session;
		[Export ("audioSessionMediaServerTerminated:")]
		void AudioSessionMediaServerTerminated (RTCAudioSession session);

		// @optional -(void)audioSessionMediaServerReset:(RTCAudioSession * _Nonnull)session;
		[Export ("audioSessionMediaServerReset:")]
		void AudioSessionMediaServerReset (RTCAudioSession session);

		// @optional -(void)audioSession:(RTCAudioSession * _Nonnull)session didChangeCanPlayOrRecord:(BOOL)canPlayOrRecord;
		[Export ("audioSession:didChangeCanPlayOrRecord:")]
		void AudioSessionDidChangeCanPlayOrRecord(RTCAudioSession session, bool canPlayOrRecord);

		// @optional -(void)audioSessionDidStartPlayOrRecord:(RTCAudioSession * _Nonnull)session;
		[Export ("audioSessionDidStartPlayOrRecord:")]
		void AudioSessionDidStartPlayOrRecord (RTCAudioSession session);

		// @optional -(void)audioSessionDidStopPlayOrRecord:(RTCAudioSession * _Nonnull)session;
		[Export ("audioSessionDidStopPlayOrRecord:")]
		void AudioSessionDidStopPlayOrRecord (RTCAudioSession session);

		// @optional -(void)audioSession:(RTCAudioSession * _Nonnull)audioSession didChangeOutputVolume:(float)outputVolume;
		[Export ("audioSession:didChangeOutputVolume:")]
		void AudioSessionDidChangeOutputVolume(RTCAudioSession audioSession, float outputVolume);

		// @optional -(void)audioSession:(RTCAudioSession * _Nonnull)audioSession didDetectPlayoutGlitch:(int64_t)totalNumberOfGlitches;
		[Export ("audioSession:didDetectPlayoutGlitch:")]
		void AudioSessionDidDetectPlayoutGlitch(RTCAudioSession audioSession, long totalNumberOfGlitches);

		// @optional -(void)audioSession:(RTCAudioSession * _Nonnull)audioSession willSetActive:(BOOL)active;
		[Export ("audioSession:willSetActive:")]
		void AudioSessionWillSetActive(RTCAudioSession audioSession, bool active);

		// @optional -(void)audioSession:(RTCAudioSession * _Nonnull)audioSession didSetActive:(BOOL)active;
		[Export ("audioSession:didSetActive:")]
		void AudioSessionDidSetActive(RTCAudioSession audioSession, bool active);

		// @optional -(void)audioSession:(RTCAudioSession * _Nonnull)audioSession failedToSetActive:(BOOL)active error:(NSError * _Nonnull)error;
		[Export ("audioSession:failedToSetActive:error:")]
		void AudioSessionFailedToSetActive(RTCAudioSession audioSession, bool active, NSError error);

		// @optional -(void)audioSession:(RTCAudioSession * _Nonnull)audioSession audioUnitStartFailedWithError:(NSError * _Nonnull)error;
		[Export ("audioSession:audioUnitStartFailedWithError:")]
		void AudioSessionAudioUnitStartFailedWithError(RTCAudioSession audioSession, NSError error);
	}

	// @protocol RTCAudioSessionActivationDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface RTCAudioSessionActivationDelegate
	{
		// @required -(void)audioSessionDidActivate:(AVAudioSession * _Nonnull)session;
		[Abstract]
		[Export ("audioSessionDidActivate:")]
		void AudioSessionDidActivate (AVAudioSession session);

		// @required -(void)audioSessionDidDeactivate:(AVAudioSession * _Nonnull)session;
		[Abstract]
		[Export ("audioSessionDidDeactivate:")]
		void AudioSessionDidDeactivate (AVAudioSession session);
	}

	// @interface RTCAudioSession : NSObject <RTCAudioSessionActivationDelegate>
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCAudioSession : IRTCAudioSessionActivationDelegate
	{
		// @property (readonly, nonatomic) AVAudioSession * _Nonnull session;
		[Export ("session")]
		AVAudioSession Session { get; }

		// @property (readonly, nonatomic) BOOL isActive;
		[Export ("isActive")]
		bool IsActive { get; }

		// @property (assign, nonatomic) BOOL useManualAudio;
		[Export ("useManualAudio")]
		bool UseManualAudio { get; set; }

		// @property (assign, nonatomic) BOOL isAudioEnabled;
		[Export ("isAudioEnabled")]
		bool IsAudioEnabled { get; set; }

		// @property (readonly) NSString * _Nonnull category;
		[Export ("category")]
		string Category { get; }

		// @property (readonly) AVAudioSessionCategoryOptions categoryOptions;
		[Export ("categoryOptions")]
		AVAudioSessionCategoryOptions CategoryOptions { get; }

		// @property (readonly) NSString * _Nonnull mode;
		[Export ("mode")]
		string Mode { get; }

		// @property (readonly) BOOL secondaryAudioShouldBeSilencedHint;
		[Export ("secondaryAudioShouldBeSilencedHint")]
		bool SecondaryAudioShouldBeSilencedHint { get; }

		// @property (readonly) AVAudioSessionRouteDescription * _Nonnull currentRoute;
		[Export ("currentRoute")]
		AVAudioSessionRouteDescription CurrentRoute { get; }

		// @property (readonly) NSInteger maximumInputNumberOfChannels;
		[Export ("maximumInputNumberOfChannels")]
		nint MaximumInputNumberOfChannels { get; }

		// @property (readonly) NSInteger maximumOutputNumberOfChannels;
		[Export ("maximumOutputNumberOfChannels")]
		nint MaximumOutputNumberOfChannels { get; }

		// @property (readonly) float inputGain;
		[Export ("inputGain")]
		float InputGain { get; }

		// @property (readonly) BOOL inputGainSettable;
		[Export ("inputGainSettable")]
		bool InputGainSettable { get; }

		// @property (readonly) BOOL inputAvailable;
		[Export ("inputAvailable")]
		bool InputAvailable { get; }

		// @property (readonly) NSArray<AVAudioSessionDataSourceDescription *> * _Nullable inputDataSources;
		[NullAllowed, Export ("inputDataSources")]
		AVAudioSessionDataSourceDescription[] InputDataSources { get; }

		[Wrap ("WeakInputDataSource")]
		[NullAllowed]
		AVAudioSessionDataSourceDescription InputDataSource { get; }

		// @property (readonly) AVAudioSessionDataSourceDescription * _Nullable inputDataSource;
		[NullAllowed, Export ("inputDataSource")]
		NSObject WeakInputDataSource { get; }

		// @property (readonly) NSArray<AVAudioSessionDataSourceDescription *> * _Nullable outputDataSources;
		[NullAllowed, Export ("outputDataSources")]
		AVAudioSessionDataSourceDescription[] OutputDataSources { get; }

		[Wrap ("WeakOutputDataSource")]
		[NullAllowed]
		AVAudioSessionDataSourceDescription OutputDataSource { get; }

		// @property (readonly) AVAudioSessionDataSourceDescription * _Nullable outputDataSource;
		[NullAllowed, Export ("outputDataSource")]
		NSObject WeakOutputDataSource { get; }

		// @property (readonly) double sampleRate;
		[Export ("sampleRate")]
		double SampleRate { get; }

		// @property (readonly) double preferredSampleRate;
		[Export ("preferredSampleRate")]
		double PreferredSampleRate { get; }

		// @property (readonly) NSInteger inputNumberOfChannels;
		[Export ("inputNumberOfChannels")]
		nint InputNumberOfChannels { get; }

		// @property (readonly) NSInteger outputNumberOfChannels;
		[Export ("outputNumberOfChannels")]
		nint OutputNumberOfChannels { get; }

		// @property (readonly) float outputVolume;
		[Export ("outputVolume")]
		float OutputVolume { get; }

		// @property (readonly) NSTimeInterval inputLatency;
		[Export ("inputLatency")]
		double InputLatency { get; }

		// @property (readonly) NSTimeInterval outputLatency;
		[Export ("outputLatency")]
		double OutputLatency { get; }

		// @property (readonly) NSTimeInterval IOBufferDuration;
		[Export ("IOBufferDuration")]
		double IOBufferDuration { get; }

		// @property (readonly) NSTimeInterval preferredIOBufferDuration;
		[Export ("preferredIOBufferDuration")]
		double PreferredIOBufferDuration { get; }

		// @property (nonatomic) BOOL ignoresPreferredAttributeConfigurationErrors;
		[Export ("ignoresPreferredAttributeConfigurationErrors")]
		bool IgnoresPreferredAttributeConfigurationErrors { get; set; }

		// +(instancetype _Nonnull)sharedInstance;
		[Static]
		[Export ("sharedInstance")]
		RTCAudioSession SharedInstance ();

		// -(void)addDelegate:(id<RTCAudioSessionDelegate> _Nonnull)delegate;
		[Export ("addDelegate:")]
		void AddDelegate (RTCAudioSessionDelegate @delegate);

		// -(void)removeDelegate:(id<RTCAudioSessionDelegate> _Nonnull)delegate;
		[Export ("removeDelegate:")]
		void RemoveDelegate (RTCAudioSessionDelegate @delegate);

		// -(void)lockForConfiguration;
		[Export ("lockForConfiguration")]
		void LockForConfiguration ();

		// -(void)unlockForConfiguration;
		[Export ("unlockForConfiguration")]
		void UnlockForConfiguration ();

		// -(BOOL)setActive:(BOOL)active error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setActive:error:")]
		bool SetActive (bool active, [NullAllowed] out NSError outError);

		// -(BOOL)setCategory:(AVAudioSessionCategory _Nonnull)category mode:(AVAudioSessionMode _Nonnull)mode options:(AVAudioSessionCategoryOptions)options error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setCategory:mode:options:error:")]
		bool SetCategory (string category, string mode, AVAudioSessionCategoryOptions options, [NullAllowed] out NSError outError);

		// -(BOOL)setCategory:(AVAudioSessionCategory _Nonnull)category withOptions:(AVAudioSessionCategoryOptions)options error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setCategory:withOptions:error:")]
		bool SetCategory (string category, AVAudioSessionCategoryOptions options, [NullAllowed] out NSError outError);

		// -(BOOL)setMode:(AVAudioSessionMode _Nonnull)mode error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setMode:error:")]
		bool SetMode (string mode, [NullAllowed] out NSError outError);

		// -(BOOL)setInputGain:(float)gain error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setInputGain:error:")]
		bool SetInputGain (float gain, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredSampleRate:(double)sampleRate error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredSampleRate:error:")]
		bool SetPreferredSampleRate (double sampleRate, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredIOBufferDuration:(NSTimeInterval)duration error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredIOBufferDuration:error:")]
		bool SetPreferredIOBufferDuration (double duration, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredInputNumberOfChannels:(NSInteger)count error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredInputNumberOfChannels:error:")]
		bool SetPreferredInputNumberOfChannels (nint count, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredOutputNumberOfChannels:(NSInteger)count error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredOutputNumberOfChannels:error:")]
		bool SetPreferredOutputNumberOfChannels (nint count, [NullAllowed] out NSError outError);

		// -(BOOL)overrideOutputAudioPort:(AVAudioSessionPortOverride)portOverride error:(NSError * _Nullable * _Nullable)outError;
		[Export ("overrideOutputAudioPort:error:")]
		bool OverrideOutputAudioPort (AVAudioSessionPortOverride portOverride, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredInput:(AVAudioSessionPortDescription * _Nonnull)inPort error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredInput:error:")]
		bool SetPreferredInput (AVAudioSessionPortDescription inPort, [NullAllowed] out NSError outError);

		// -(BOOL)setInputDataSource:(AVAudioSessionDataSourceDescription * _Nonnull)dataSource error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setInputDataSource:error:")]
		bool SetInputDataSource (AVAudioSessionDataSourceDescription dataSource, [NullAllowed] out NSError outError);

		// -(BOOL)setOutputDataSource:(AVAudioSessionDataSourceDescription * _Nonnull)dataSource error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setOutputDataSource:error:")]
		bool SetOutputDataSource (AVAudioSessionDataSourceDescription dataSource, [NullAllowed] out NSError outError);
	}

	// @interface Configuration (RTCAudioSession)
	[Category]
	[BaseType (typeof(RTCAudioSession))]
	interface RTCAudioSession_Configuration
	{
		// -(BOOL)setConfiguration:(RTCAudioSessionConfiguration * _Nonnull)configuration error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setConfiguration:error:")]
		bool SetConfiguration (RTCAudioSessionConfiguration configuration, [NullAllowed] out NSError outError);

		// -(BOOL)setConfiguration:(RTCAudioSessionConfiguration * _Nonnull)configuration active:(BOOL)active error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setConfiguration:active:error:")]
		bool SetConfiguration (RTCAudioSessionConfiguration configuration, bool active, [NullAllowed] out NSError outError);
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern const int kRTCAudioSessionPreferredNumberOfChannels;
		[Field ("kRTCAudioSessionPreferredNumberOfChannels", "__Internal")]
		int kRTCAudioSessionPreferredNumberOfChannels { get; }

		// extern const double kRTCAudioSessionHighPerformanceSampleRate;
		[Field ("kRTCAudioSessionHighPerformanceSampleRate", "__Internal")]
		double kRTCAudioSessionHighPerformanceSampleRate { get; }

		// extern const double kRTCAudioSessionHighPerformanceIOBufferDuration;
		[Field ("kRTCAudioSessionHighPerformanceIOBufferDuration", "__Internal")]
		double kRTCAudioSessionHighPerformanceIOBufferDuration { get; }
	}

	// @interface RTCAudioSessionConfiguration : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCAudioSessionConfiguration
	{
		// @property (nonatomic, strong) NSString * _Nonnull category;
		[Export ("category", ArgumentSemantic.Strong)]
		string Category { get; set; }

		// @property (assign, nonatomic) AVAudioSessionCategoryOptions categoryOptions;
		[Export ("categoryOptions", ArgumentSemantic.Assign)]
		AVAudioSessionCategoryOptions CategoryOptions { get; set; }

		// @property (nonatomic, strong) NSString * _Nonnull mode;
		[Export ("mode", ArgumentSemantic.Strong)]
		string Mode { get; set; }

		// @property (assign, nonatomic) double sampleRate;
		[Export ("sampleRate")]
		double SampleRate { get; set; }

		// @property (assign, nonatomic) NSTimeInterval ioBufferDuration;
		[Export ("ioBufferDuration")]
		double IoBufferDuration { get; set; }

		// @property (assign, nonatomic) NSInteger inputNumberOfChannels;
		[Export ("inputNumberOfChannels")]
		nint InputNumberOfChannels { get; set; }

		// @property (assign, nonatomic) NSInteger outputNumberOfChannels;
		[Export ("outputNumberOfChannels")]
		nint OutputNumberOfChannels { get; set; }

		// +(instancetype _Nonnull)currentConfiguration;
		[Static]
		[Export ("currentConfiguration")]
		RTCAudioSessionConfiguration CurrentConfiguration ();

		// +(instancetype _Nonnull)webRTCConfiguration;
		[Static]
		[Export ("webRTCConfiguration")]
		RTCAudioSessionConfiguration WebRTCConfiguration ();

		// +(void)setWebRTCConfiguration:(RTCAudioSessionConfiguration * _Nonnull)configuration;
		[Static]
		[Export ("setWebRTCConfiguration:")]
		void SetWebRTCConfiguration (RTCAudioSessionConfiguration configuration);
	}

	// @interface RTCCameraVideoCapturer : RTCVideoCapturer
	////[Unavailable (PlatformName.iOSAppExtension)]
	[BaseType (typeof(RTCVideoCapturer))]
	interface RTCCameraVideoCapturer
	{
		// @property (readonly, nonatomic) AVCaptureSession * _Nonnull captureSession;
		[Export ("captureSession")]
		AVCaptureSession CaptureSession { get; }

		// +(NSArray<AVCaptureDevice *> * _Nonnull)captureDevices;
		[Static]
		[Export ("captureDevices")]
		////[Verify (MethodToProperty)]
		AVCaptureDevice[] CaptureDevices { get; }

		// +(NSArray<AVCaptureDeviceFormat *> * _Nonnull)supportedFormatsForDevice:(AVCaptureDevice * _Nonnull)device;
		[Static]
		[Export ("supportedFormatsForDevice:")]
		AVCaptureDeviceFormat[] SupportedFormatsForDevice (AVCaptureDevice device);

		// -(FourCharCode)preferredOutputPixelFormat;
		[Export ("preferredOutputPixelFormat")]
		////[Verify (MethodToProperty)]
		uint PreferredOutputPixelFormat { get; }

		// -(void)startCaptureWithDevice:(AVCaptureDevice * _Nonnull)device format:(AVCaptureDeviceFormat * _Nonnull)format fps:(NSInteger)fps completionHandler:(void (^ _Nullable)(NSError * _Nullable))completionHandler;
		[Export ("startCaptureWithDevice:format:fps:completionHandler:")]
		void StartCaptureWithDevice (AVCaptureDevice device, AVCaptureDeviceFormat format, nint fps, [NullAllowed] Action<NSError> completionHandler);

		// -(void)stopCaptureWithCompletionHandler:(void (^ _Nullable)(void))completionHandler;
		[Export ("stopCaptureWithCompletionHandler:")]
		void StopCaptureWithCompletionHandler ([NullAllowed] Action completionHandler);

		// -(void)startCaptureWithDevice:(AVCaptureDevice * _Nonnull)device format:(AVCaptureDeviceFormat * _Nonnull)format fps:(NSInteger)fps;
		[Export ("startCaptureWithDevice:format:fps:")]
		void StartCaptureWithDevice (AVCaptureDevice device, AVCaptureDeviceFormat format, nint fps);

		// -(void)stopCapture;
		[Export ("stopCapture")]
		void StopCapture ();
	}

	// typedef void (^RTCFileVideoCapturerErrorBlock)(NSError * _Nonnull);
	delegate void RTCFileVideoCapturerErrorBlock (NSError arg0);

	// @interface RTCFileVideoCapturer : RTCVideoCapturer
	////[iOS (10,0)]
	[BaseType (typeof(RTCVideoCapturer))]
	interface RTCFileVideoCapturer
	{
		// -(void)startCapturingFromFileNamed:(NSString * _Nonnull)nameOfFile onError:(RTCFileVideoCapturerErrorBlock _Nullable)errorBlock;
		[Export ("startCapturingFromFileNamed:onError:")]
		void StartCapturingFromFileNamed (string nameOfFile, [NullAllowed] RTCFileVideoCapturerErrorBlock errorBlock);

		// -(void)stopCapture;
		[Export ("stopCapture")]
		void StopCapture ();
	}

	// @interface RTCNetworkMonitor : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
[Protocol]
	interface RTCNetworkMonitor
	{
	}

	// @interface RTCMTLVideoView : UIView <RTCVideoRenderer>
	////[iOS (9,0)]
	[BaseType (typeof(UIView))]
	interface RTCMTLVideoView : /****I****/RTCVideoRenderer
	{
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		IRTCVideoViewDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<RTCVideoViewDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// @property (nonatomic) UIViewContentMode videoContentMode;
		[Export ("videoContentMode", ArgumentSemantic.Assign)]
		UIViewContentMode VideoContentMode { get; set; }

		// @property (getter = isEnabled, nonatomic) BOOL enabled;
		[Export ("enabled")]
		bool Enabled { [Bind ("isEnabled")] get; set; }

		// @property (nonatomic) NSValue * _Nullable rotationOverride;
		[NullAllowed, Export ("rotationOverride", ArgumentSemantic.Assign)]
		NSValue RotationOverride { get; set; }
	}

	// @protocol RTCVideoViewShading <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCVideoViewShading
	{
		// @required -(void)applyShadingForFrameWithWidth:(int)width height:(int)height rotation:(RTCVideoRotation)rotation yPlane:(GLuint)yPlane uPlane:(GLuint)uPlane vPlane:(GLuint)vPlane;
		[Abstract]
		[Export ("applyShadingForFrameWithWidth:height:rotation:yPlane:uPlane:vPlane:")]
		void Height (int width, int height, RTCVideoRotation rotation, uint yPlane, uint uPlane, uint vPlane);

		// @required -(void)applyShadingForFrameWithWidth:(int)width height:(int)height rotation:(RTCVideoRotation)rotation yPlane:(GLuint)yPlane uvPlane:(GLuint)uvPlane;
		[Abstract]
		[Export ("applyShadingForFrameWithWidth:height:rotation:yPlane:uvPlane:")]
		void Height (int width, int height, RTCVideoRotation rotation, uint yPlane, uint uvPlane);
	}

    // @interface RTCEAGLVideoView : UIView <RTCVideoRenderer>
    ////[Unavailable (PlatformName.iOSAppExtension)]
 [Protocol]
    [BaseType (typeof(UIView))]
	interface RTCEAGLVideoView : /****I****/RTCVideoRenderer
	{
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		IRTCVideoViewDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<RTCVideoViewDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// -(instancetype _Nonnull)initWithFrame:(CGRect)frame shader:(id<RTCVideoViewShading> _Nonnull)shader __attribute__((objc_designated_initializer));
		[Export ("initWithFrame:shader:")]
		[DesignatedInitializer]
		NativeHandle Constructor (CGRect frame, RTCVideoViewShading shader);

		// -(instancetype _Nonnull)initWithCoder:(NSCoder * _Nonnull)aDecoder shader:(id<RTCVideoViewShading> _Nonnull)shader __attribute__((objc_designated_initializer));
		[Export ("initWithCoder:shader:")]
		[DesignatedInitializer]
		NativeHandle Constructor (NSCoder aDecoder, RTCVideoViewShading shader);

		// @property (nonatomic) NSValue * _Nullable rotationOverride;
		[NullAllowed, Export ("rotationOverride", ArgumentSemantic.Assign)]
		NSValue RotationOverride { get; set; }
	}

	// @interface RTCCodecSpecificInfoH264 : NSObject <RTCCodecSpecificInfo>
	[BaseType (typeof(NSObject))]
	interface RTCCodecSpecificInfoH264 : IRTCCodecSpecificInfo
	{
		// @property (assign, nonatomic) RTCH264PacketizationMode packetizationMode;
		[Export ("packetizationMode", ArgumentSemantic.Assign)]
		RTCH264PacketizationMode PacketizationMode { get; set; }
	}

	// @interface RTCDefaultVideoDecoderFactory : NSObject <RTCVideoDecoderFactory>
	////[BaseType (typeof(NSObject))]
	[BaseType (typeof(RTCVideoDecoderFactory))]
    interface RTCDefaultVideoDecoderFactory ////: IRTCVideoDecoderFactory
	{
	}

    // @interface RTCDefaultVideoEncoderFactory : NSObject <RTCVideoEncoderFactory>
    ////[BaseType (typeof(NSObject))]
    [BaseType (typeof(RTCVideoEncoderFactory))]
    interface RTCDefaultVideoEncoderFactory ////: IRTCVideoEncoderFactory
	{
		// @property (retain, nonatomic) RTCVideoCodecInfo * _Nonnull preferredCodec;
		[Export ("preferredCodec", ArgumentSemantic.Retain)]
		RTCVideoCodecInfo PreferredCodec { get; set; }

		// +(NSArray<RTCVideoCodecInfo *> * _Nonnull)supportedCodecs;
		[Static]
		[Export ("supportedCodecs")]
		////[Verify (MethodToProperty)]
		RTCVideoCodecInfo[] SupportedCodecs { get; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const kRTCVideoCodecH264Name;
		[Field ("kRTCVideoCodecH264Name", "__Internal")]
		NSString kRTCVideoCodecH264Name { get; }

		// extern NSString *const kRTCLevel31ConstrainedHigh;
		[Field ("kRTCLevel31ConstrainedHigh", "__Internal")]
		NSString kRTCLevel31ConstrainedHigh { get; }

		// extern NSString *const kRTCLevel31ConstrainedBaseline;
		[Field ("kRTCLevel31ConstrainedBaseline", "__Internal")]
		NSString kRTCLevel31ConstrainedBaseline { get; }

		// extern NSString *const kRTCMaxSupportedH264ProfileLevelConstrainedHigh;
		[Field ("kRTCMaxSupportedH264ProfileLevelConstrainedHigh", "__Internal")]
		NSString kRTCMaxSupportedH264ProfileLevelConstrainedHigh { get; }

		// extern NSString *const kRTCMaxSupportedH264ProfileLevelConstrainedBaseline;
		[Field ("kRTCMaxSupportedH264ProfileLevelConstrainedBaseline", "__Internal")]
		NSString kRTCMaxSupportedH264ProfileLevelConstrainedBaseline { get; }
	}

	// @interface RTCH264ProfileLevelId : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCH264ProfileLevelId
	{
		// @property (readonly, nonatomic) RTCH264Profile profile;
		[Export ("profile")]
		RTCH264Profile Profile { get; }

		// @property (readonly, nonatomic) RTCH264Level level;
		[Export ("level")]
		RTCH264Level Level { get; }

		// @property (readonly, nonatomic) NSString * hexString;
		[Export ("hexString")]
		string HexString { get; }

		// -(instancetype)initWithHexString:(NSString *)hexString;
		[Export ("initWithHexString:")]
		NativeHandle Constructor (string hexString);

		// -(instancetype)initWithProfile:(RTCH264Profile)profile level:(RTCH264Level)level;
		[Export ("initWithProfile:level:")]
		NativeHandle Constructor (RTCH264Profile profile, RTCH264Level level);
	}

	// @interface RTCVideoDecoderFactoryH264 : NSObject <RTCVideoDecoderFactory>
	[BaseType (typeof(NSObject))]
	interface RTCVideoDecoderFactoryH264 : IRTCVideoDecoderFactory
	{
	}

	// @interface RTCVideoDecoderH264 : NSObject <RTCVideoDecoder>
	[BaseType (typeof(NSObject))]
	interface RTCVideoDecoderH264 : IRTCVideoDecoder
	{
	}

	// @interface RTCVideoEncoderFactoryH264 : NSObject <RTCVideoEncoderFactory>
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderFactoryH264 : IRTCVideoEncoderFactory
	{
	}

	// @interface RTCVideoEncoderH264 : NSObject <RTCVideoEncoder>
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderH264 : IRTCVideoEncoder
	{
		// -(instancetype)initWithCodecInfo:(RTCVideoCodecInfo *)codecInfo;
		[Export ("initWithCodecInfo:")]
		NativeHandle Constructor (RTCVideoCodecInfo codecInfo);
	}

	// @interface RTCCVPixelBuffer : NSObject <RTCVideoFrameBuffer>
	[BaseType (typeof(NSObject))]
	interface RTCCVPixelBuffer : IRTCVideoFrameBuffer
	{
		// @property (readonly, nonatomic) CVPixelBufferRef _Nonnull pixelBuffer;
		[Export ("pixelBuffer")]
		CVPixelBuffer PixelBuffer { get; }

		// @property (readonly, nonatomic) int cropX;
		[Export ("cropX")]
		int CropX { get; }

		// @property (readonly, nonatomic) int cropY;
		[Export ("cropY")]
		int CropY { get; }

		// @property (readonly, nonatomic) int cropWidth;
		[Export ("cropWidth")]
		int CropWidth { get; }

		// @property (readonly, nonatomic) int cropHeight;
		[Export ("cropHeight")]
		int CropHeight { get; }

		// +(NSSet<NSNumber *> * _Nonnull)supportedPixelFormats;
		[Static]
		[Export ("supportedPixelFormats")]
		////[Verify (MethodToProperty)]
		NSSet<NSNumber> SupportedPixelFormats { get; }

		// -(instancetype _Nonnull)initWithPixelBuffer:(CVPixelBufferRef _Nonnull)pixelBuffer;
		[Export ("initWithPixelBuffer:")]
		NativeHandle Constructor (CVPixelBuffer pixelBuffer);

		// -(instancetype _Nonnull)initWithPixelBuffer:(CVPixelBufferRef _Nonnull)pixelBuffer adaptedWidth:(int)adaptedWidth adaptedHeight:(int)adaptedHeight cropWidth:(int)cropWidth cropHeight:(int)cropHeight cropX:(int)cropX cropY:(int)cropY;
		[Export ("initWithPixelBuffer:adaptedWidth:adaptedHeight:cropWidth:cropHeight:cropX:cropY:")]
		NativeHandle Constructor (CVPixelBuffer pixelBuffer, int adaptedWidth, int adaptedHeight, int cropWidth, int cropHeight, int cropX, int cropY);

		// -(BOOL)requiresCropping;
		[Export ("requiresCropping")]
		////[Verify (MethodToProperty)]
		bool RequiresCropping { get; }

		// -(BOOL)requiresScalingToWidth:(int)width height:(int)height;
		[Export ("requiresScalingToWidth:height:")]
		bool RequiresScalingToWidth (int width, int height);

		// -(int)bufferSizeForCroppingAndScalingToWidth:(int)width height:(int)height;
		[Export ("bufferSizeForCroppingAndScalingToWidth:height:")]
		int BufferSizeForCroppingAndScalingToWidth (int width, int height);

		// -(BOOL)cropAndScaleTo:(CVPixelBufferRef _Nonnull)outputPixelBuffer withTempBuffer:(uint8_t * _Nullable)tmpBuffer;
		[Export ("cropAndScaleTo:withTempBuffer:")]
		/****unsafe****/ bool CropAndScaleTo (CVPixelBuffer outputPixelBuffer, [NullAllowed] /****byte* ****/IntPtr tmpBuffer);
	}

	// @interface RTCCameraPreviewView : UIView
	[BaseType (typeof(UIView))]
	interface RTCCameraPreviewView
	{
		// @property (nonatomic, strong) AVCaptureSession * captureSession;
		[Export ("captureSession", ArgumentSemantic.Strong)]
		AVCaptureSession CaptureSession { get; set; }
	}

	// @interface RTCDispatcher : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCDispatcher
	{
		// +(void)dispatchAsyncOnType:(RTCDispatcherQueueType)dispatchType block:(dispatch_block_t)block;
		[Static]
		[Export ("dispatchAsyncOnType:block:")]
		void DispatchAsyncOnType (RTCDispatcherQueueType dispatchType, dispatch_block_t block);

		// +(BOOL)isOnQueueForType:(RTCDispatcherQueueType)dispatchType;
		[Static]
		[Export ("isOnQueueForType:")]
		bool IsOnQueueForType (RTCDispatcherQueueType dispatchType);
	}

	// @interface RTCDevice (UIDevice)
	[Category]
	[BaseType (typeof(UIDevice))]
	interface UIDevice_RTCDevice
	{
		// +(NSString *)machineName;
		[Static]
		[Export ("machineName")]
		////[Verify (MethodToProperty)]
		string MachineName { get; }
	}

	// @interface RTCMediaSource : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCMediaSource
	{
		// @property (readonly, nonatomic) RTCSourceState state;
		[Export ("state")]
		RTCSourceState State { get; }
	}

	// @interface RTCAudioSource : RTCMediaSource
	[BaseType (typeof(RTCMediaSource))]
	[DisableDefaultCtor]
	interface RTCAudioSource
	{
		// @property (assign, nonatomic) double volume;
		[Export ("volume")]
		double Volume { get; set; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const _Nonnull kRTCMediaStreamTrackKindAudio;
		[Field ("kRTCMediaStreamTrackKindAudio", "__Internal")]
		NSString kRTCMediaStreamTrackKindAudio { get; }

		// extern NSString *const _Nonnull kRTCMediaStreamTrackKindVideo;
		[Field ("kRTCMediaStreamTrackKindVideo", "__Internal")]
		NSString kRTCMediaStreamTrackKindVideo { get; }
	}

	// @interface RTCMediaStreamTrack : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCMediaStreamTrack
	{
		// @property (readonly, nonatomic) NSString * _Nonnull kind;
		[Export ("kind")]
		string Kind { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull trackId;
		[Export ("trackId")]
		string TrackId { get; }

		// @property (assign, nonatomic) BOOL isEnabled;
		[Export ("isEnabled")]
		bool IsEnabled { get; set; }

		// @property (readonly, nonatomic) RTCMediaStreamTrackState readyState;
		[Export ("readyState")]
		RTCMediaStreamTrackState ReadyState { get; }
	}

	// @interface RTCAudioTrack : RTCMediaStreamTrack
	[BaseType (typeof(RTCMediaStreamTrack))]
	[DisableDefaultCtor]
	interface RTCAudioTrack
	{
		// @property (readonly, nonatomic) RTCAudioSource * _Nonnull source;
		[Export ("source")]
		RTCAudioSource Source { get; }
	}

	// @interface RTCCertificate : NSObject <NSCopying>
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCCertificate : INSCopying
	{
		// @property (readonly, copy, nonatomic) NSString * _Nonnull private_key;
		[Export ("private_key")]
		string Private_key { get; }

		// @property (readonly, copy, nonatomic) NSString * _Nonnull certificate;
		[Export ("certificate")]
		string Certificate { get; }

		// -(instancetype _Nonnull)initWithPrivateKey:(NSString * _Nonnull)private_key certificate:(NSString * _Nonnull)certificate __attribute__((objc_designated_initializer));
		[Export ("initWithPrivateKey:certificate:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string private_key, string certificate);

		// +(RTCCertificate * _Nullable)generateCertificateWithParams:(NSDictionary * _Nonnull)params;
		[Static]
		[Export ("generateCertificateWithParams:")]
		[return: NullAllowed]
		RTCCertificate GenerateCertificateWithParams (NSDictionary @params);
	}

	// @interface RTCCryptoOptions : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCCryptoOptions
	{
		// @property (assign, nonatomic) BOOL srtpEnableGcmCryptoSuites;
		[Export ("srtpEnableGcmCryptoSuites")]
		bool SrtpEnableGcmCryptoSuites { get; set; }

		// @property (assign, nonatomic) BOOL srtpEnableAes128Sha1_32CryptoCipher;
		[Export ("srtpEnableAes128Sha1_32CryptoCipher")]
		bool SrtpEnableAes128Sha1_32CryptoCipher { get; set; }

		// @property (assign, nonatomic) BOOL srtpEnableEncryptedRtpHeaderExtensions;
		[Export ("srtpEnableEncryptedRtpHeaderExtensions")]
		bool SrtpEnableEncryptedRtpHeaderExtensions { get; set; }

		// @property (assign, nonatomic) BOOL sframeRequireFrameEncryption;
		[Export ("sframeRequireFrameEncryption")]
		bool SframeRequireFrameEncryption { get; set; }

		// -(instancetype _Nonnull)initWithSrtpEnableGcmCryptoSuites:(BOOL)srtpEnableGcmCryptoSuites srtpEnableAes128Sha1_32CryptoCipher:(BOOL)srtpEnableAes128Sha1_32CryptoCipher srtpEnableEncryptedRtpHeaderExtensions:(BOOL)srtpEnableEncryptedRtpHeaderExtensions sframeRequireFrameEncryption:(BOOL)sframeRequireFrameEncryption __attribute__((objc_designated_initializer));
		[Export ("initWithSrtpEnableGcmCryptoSuites:srtpEnableAes128Sha1_32CryptoCipher:srtpEnableEncryptedRtpHeaderExtensions:sframeRequireFrameEncryption:")]
		[DesignatedInitializer]
		NativeHandle Constructor (bool srtpEnableGcmCryptoSuites, bool srtpEnableAes128Sha1_32CryptoCipher, bool srtpEnableEncryptedRtpHeaderExtensions, bool sframeRequireFrameEncryption);
	}

	// @interface RTCConfiguration : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCConfiguration
	{
		// @property (assign, nonatomic) BOOL enableDscp;
		[Export ("enableDscp")]
		bool EnableDscp { get; set; }

		// @property (copy, nonatomic) NSArray<RTCIceServer *> * _Nonnull iceServers;
		[Export ("iceServers", ArgumentSemantic.Copy)]
		RTCIceServer[] IceServers { get; set; }

		// @property (nonatomic) RTCCertificate * _Nullable certificate;
		[NullAllowed, Export ("certificate", ArgumentSemantic.Assign)]
		RTCCertificate Certificate { get; set; }

		// @property (assign, nonatomic) RTCIceTransportPolicy iceTransportPolicy;
		[Export ("iceTransportPolicy", ArgumentSemantic.Assign)]
		RTCIceTransportPolicy IceTransportPolicy { get; set; }

		// @property (assign, nonatomic) RTCBundlePolicy bundlePolicy;
		[Export ("bundlePolicy", ArgumentSemantic.Assign)]
		RTCBundlePolicy BundlePolicy { get; set; }

		// @property (assign, nonatomic) RTCRtcpMuxPolicy rtcpMuxPolicy;
		[Export ("rtcpMuxPolicy", ArgumentSemantic.Assign)]
		RTCRtcpMuxPolicy RtcpMuxPolicy { get; set; }

		// @property (assign, nonatomic) RTCTcpCandidatePolicy tcpCandidatePolicy;
		[Export ("tcpCandidatePolicy", ArgumentSemantic.Assign)]
		RTCTcpCandidatePolicy TcpCandidatePolicy { get; set; }

		// @property (assign, nonatomic) RTCCandidateNetworkPolicy candidateNetworkPolicy;
		[Export ("candidateNetworkPolicy", ArgumentSemantic.Assign)]
		RTCCandidateNetworkPolicy CandidateNetworkPolicy { get; set; }

		// @property (assign, nonatomic) RTCContinualGatheringPolicy continualGatheringPolicy;
		[Export ("continualGatheringPolicy", ArgumentSemantic.Assign)]
		RTCContinualGatheringPolicy ContinualGatheringPolicy { get; set; }

		// @property (assign, nonatomic) BOOL disableIPV6OnWiFi;
		[Export ("disableIPV6OnWiFi")]
		bool DisableIPV6OnWiFi { get; set; }

		// @property (assign, nonatomic) int maxIPv6Networks;
		[Export ("maxIPv6Networks")]
		int MaxIPv6Networks { get; set; }

		// @property (assign, nonatomic) BOOL disableLinkLocalNetworks;
		[Export ("disableLinkLocalNetworks")]
		bool DisableLinkLocalNetworks { get; set; }

		// @property (assign, nonatomic) int audioJitterBufferMaxPackets;
		[Export ("audioJitterBufferMaxPackets")]
		int AudioJitterBufferMaxPackets { get; set; }

		// @property (assign, nonatomic) BOOL audioJitterBufferFastAccelerate;
		[Export ("audioJitterBufferFastAccelerate")]
		bool AudioJitterBufferFastAccelerate { get; set; }

		// @property (assign, nonatomic) int iceConnectionReceivingTimeout;
		[Export ("iceConnectionReceivingTimeout")]
		int IceConnectionReceivingTimeout { get; set; }

		// @property (assign, nonatomic) int iceBackupCandidatePairPingInterval;
		[Export ("iceBackupCandidatePairPingInterval")]
		int IceBackupCandidatePairPingInterval { get; set; }

		// @property (assign, nonatomic) RTCEncryptionKeyType keyType;
		[Export ("keyType", ArgumentSemantic.Assign)]
		RTCEncryptionKeyType KeyType { get; set; }

		// @property (assign, nonatomic) int iceCandidatePoolSize;
		[Export ("iceCandidatePoolSize")]
		int IceCandidatePoolSize { get; set; }

		// @property (assign, nonatomic) BOOL shouldPruneTurnPorts;
		[Export ("shouldPruneTurnPorts")]
		bool ShouldPruneTurnPorts { get; set; }

		// @property (assign, nonatomic) BOOL shouldPresumeWritableWhenFullyRelayed;
		[Export ("shouldPresumeWritableWhenFullyRelayed")]
		bool ShouldPresumeWritableWhenFullyRelayed { get; set; }

		// @property (assign, nonatomic) BOOL shouldSurfaceIceCandidatesOnIceTransportTypeChanged;
		[Export ("shouldSurfaceIceCandidatesOnIceTransportTypeChanged")]
		bool ShouldSurfaceIceCandidatesOnIceTransportTypeChanged { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceCheckMinInterval;
		[NullAllowed, Export ("iceCheckMinInterval", ArgumentSemantic.Copy)]
		NSNumber IceCheckMinInterval { get; set; }

		// @property (assign, nonatomic) RTCSdpSemantics sdpSemantics;
		[Export ("sdpSemantics", ArgumentSemantic.Assign)]
		RTCSdpSemantics SdpSemantics { get; set; }

		// @property (assign, nonatomic) BOOL activeResetSrtpParams;
		[Export ("activeResetSrtpParams")]
		bool ActiveResetSrtpParams { get; set; }

		// @property (assign, nonatomic) BOOL allowCodecSwitching;
		[Export ("allowCodecSwitching")]
		bool AllowCodecSwitching { get; set; }

		// @property (nonatomic) RTCCryptoOptions * _Nullable cryptoOptions;
		[NullAllowed, Export ("cryptoOptions", ArgumentSemantic.Assign)]
		RTCCryptoOptions CryptoOptions { get; set; }

		// @property (copy, nonatomic) NSString * _Nullable turnLoggingId;
		[NullAllowed, Export ("turnLoggingId")]
		string TurnLoggingId { get; set; }

		// @property (assign, nonatomic) int rtcpAudioReportIntervalMs;
		[Export ("rtcpAudioReportIntervalMs")]
		int RtcpAudioReportIntervalMs { get; set; }

		// @property (assign, nonatomic) int rtcpVideoReportIntervalMs;
		[Export ("rtcpVideoReportIntervalMs")]
		int RtcpVideoReportIntervalMs { get; set; }

		// @property (assign, nonatomic) BOOL enableImplicitRollback;
		[Export ("enableImplicitRollback")]
		bool EnableImplicitRollback { get; set; }

		// @property (assign, nonatomic) BOOL offerExtmapAllowMixed;
		[Export ("offerExtmapAllowMixed")]
		bool OfferExtmapAllowMixed { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceCheckIntervalStrongConnectivity;
		[NullAllowed, Export ("iceCheckIntervalStrongConnectivity", ArgumentSemantic.Copy)]
		NSNumber IceCheckIntervalStrongConnectivity { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceCheckIntervalWeakConnectivity;
		[NullAllowed, Export ("iceCheckIntervalWeakConnectivity", ArgumentSemantic.Copy)]
		NSNumber IceCheckIntervalWeakConnectivity { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceUnwritableTimeout;
		[NullAllowed, Export ("iceUnwritableTimeout", ArgumentSemantic.Copy)]
		NSNumber IceUnwritableTimeout { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceUnwritableMinChecks;
		[NullAllowed, Export ("iceUnwritableMinChecks", ArgumentSemantic.Copy)]
		NSNumber IceUnwritableMinChecks { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceInactiveTimeout;
		[NullAllowed, Export ("iceInactiveTimeout", ArgumentSemantic.Copy)]
		NSNumber IceInactiveTimeout { get; set; }
	}

	// @interface RTCDataBuffer : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCDataBuffer
	{
		// @property (readonly, nonatomic) NSData * _Nonnull data;
		[Export ("data")]
		NSData Data { get; }

		// @property (readonly, nonatomic) BOOL isBinary;
		[Export ("isBinary")]
		bool IsBinary { get; }

		// -(instancetype _Nonnull)initWithData:(NSData * _Nonnull)data isBinary:(BOOL)isBinary;
		[Export ("initWithData:isBinary:")]
		NativeHandle Constructor (NSData data, bool isBinary);
	}

	// @protocol RTCDataChannelDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface RTCDataChannelDelegate
	{
		// @required -(void)dataChannelDidChangeState:(RTCDataChannel * _Nonnull)dataChannel;
		[Abstract]
		[Export ("dataChannelDidChangeState:")]
		void DataChannelDidChangeState (RTCDataChannel dataChannel);

		// @required -(void)dataChannel:(RTCDataChannel * _Nonnull)dataChannel didReceiveMessageWithBuffer:(RTCDataBuffer * _Nonnull)buffer;
		[Abstract]
		[Export ("dataChannel:didReceiveMessageWithBuffer:")]
		void DidReceiveMessageWithBuffer(RTCDataChannel dataChannel, RTCDataBuffer buffer);

		// @optional -(void)dataChannel:(RTCDataChannel * _Nonnull)dataChannel didChangeBufferedAmount:(uint64_t)amount;
		[Export ("dataChannel:didChangeBufferedAmount:")]
		void DidChangeBufferedAmount(RTCDataChannel dataChannel, ulong amount);
	}

	// @interface RTCDataChannel : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCDataChannel
	{
		// @property (readonly, nonatomic) NSString * _Nonnull label;
		[Export ("label")]
		string Label { get; }

		// @property (readonly, nonatomic) BOOL isReliable __attribute__((deprecated("")));
		[Export ("isReliable")]
		bool IsReliable { get; }

		// @property (readonly, nonatomic) BOOL isOrdered;
		[Export ("isOrdered")]
		bool IsOrdered { get; }

		// @property (readonly, nonatomic) NSUInteger maxRetransmitTime __attribute__((deprecated("")));
		[Export ("maxRetransmitTime")]
		nuint MaxRetransmitTime { get; }

		// @property (readonly, nonatomic) uint16_t maxPacketLifeTime;
		[Export ("maxPacketLifeTime")]
		ushort MaxPacketLifeTime { get; }

		// @property (readonly, nonatomic) uint16_t maxRetransmits;
		[Export ("maxRetransmits")]
		ushort MaxRetransmits { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull protocol;
		[Export ("protocol")]
		string Protocol { get; }

		// @property (readonly, nonatomic) BOOL isNegotiated;
		[Export ("isNegotiated")]
		bool IsNegotiated { get; }

		// @property (readonly, nonatomic) NSInteger streamId __attribute__((deprecated("")));
		[Export ("streamId")]
		nint StreamId { get; }

		// @property (readonly, nonatomic) int channelId;
		[Export ("channelId")]
		int ChannelId { get; }

		// @property (readonly, nonatomic) RTCDataChannelState readyState;
		[Export ("readyState")]
		RTCDataChannelState ReadyState { get; }

		// @property (readonly, nonatomic) uint64_t bufferedAmount;
		[Export ("bufferedAmount")]
		ulong BufferedAmount { get; }

		[Wrap ("WeakDelegate")]
		[NullAllowed]
		IRTCDataChannelDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<RTCDataChannelDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// -(void)close;
		[Export ("close")]
		void Close ();

		// -(BOOL)sendData:(RTCDataBuffer * _Nonnull)data;
		[Export ("sendData:")]
		bool SendData (RTCDataBuffer data);
	}

	// @interface RTCDataChannelConfiguration : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCDataChannelConfiguration
	{
		// @property (assign, nonatomic) BOOL isOrdered;
		[Export ("isOrdered")]
		bool IsOrdered { get; set; }

		// @property (assign, nonatomic) NSInteger maxRetransmitTimeMs __attribute__((deprecated("")));
		[Export ("maxRetransmitTimeMs")]
		nint MaxRetransmitTimeMs { get; set; }

		// @property (assign, nonatomic) int maxPacketLifeTime;
		[Export ("maxPacketLifeTime")]
		int MaxPacketLifeTime { get; set; }

		// @property (assign, nonatomic) int maxRetransmits;
		[Export ("maxRetransmits")]
		int MaxRetransmits { get; set; }

		// @property (assign, nonatomic) BOOL isNegotiated;
		[Export ("isNegotiated")]
		bool IsNegotiated { get; set; }

		// @property (assign, nonatomic) int streamId __attribute__((deprecated("")));
		[Export ("streamId")]
		int StreamId { get; set; }

		// @property (assign, nonatomic) int channelId;
		[Export ("channelId")]
		int ChannelId { get; set; }

		// @property (nonatomic) NSString * _Nonnull protocol;
		[Export ("protocol")]
		string Protocol { get; set; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const kRTCFieldTrialAudioForceABWENoTWCCKey;
		[Field ("kRTCFieldTrialAudioForceABWENoTWCCKey", "__Internal")]
		NSString kRTCFieldTrialAudioForceABWENoTWCCKey { get; }

		// extern NSString *const kRTCFieldTrialFlexFec03AdvertisedKey;
		[Field ("kRTCFieldTrialFlexFec03AdvertisedKey", "__Internal")]
		NSString kRTCFieldTrialFlexFec03AdvertisedKey { get; }

		// extern NSString *const kRTCFieldTrialFlexFec03Key;
		[Field ("kRTCFieldTrialFlexFec03Key", "__Internal")]
		NSString kRTCFieldTrialFlexFec03Key { get; }

		// extern NSString *const kRTCFieldTrialH264HighProfileKey;
		[Field ("kRTCFieldTrialH264HighProfileKey", "__Internal")]
		NSString kRTCFieldTrialH264HighProfileKey { get; }

		// extern NSString *const kRTCFieldTrialMinimizeResamplingOnMobileKey;
		[Field ("kRTCFieldTrialMinimizeResamplingOnMobileKey", "__Internal")]
		NSString kRTCFieldTrialMinimizeResamplingOnMobileKey { get; }

		// extern NSString *const kRTCFieldTrialUseNWPathMonitor;
		[Field ("kRTCFieldTrialUseNWPathMonitor", "__Internal")]
		NSString kRTCFieldTrialUseNWPathMonitor { get; }

		// extern NSString *const kRTCFieldTrialEnabledValue;
		[Field ("kRTCFieldTrialEnabledValue", "__Internal")]
		NSString kRTCFieldTrialEnabledValue { get; }
	}

	// @interface RTCIceCandidate : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCIceCandidate
	{
		// @property (readonly, nonatomic) NSString * _Nullable sdpMid;
		[NullAllowed, Export ("sdpMid")]
		string SdpMid { get; }

		// @property (readonly, nonatomic) int sdpMLineIndex;
		[Export ("sdpMLineIndex")]
		int SdpMLineIndex { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull sdp;
		[Export ("sdp")]
		string Sdp { get; }

		// @property (readonly, nonatomic) NSString * _Nullable serverUrl;
		[NullAllowed, Export ("serverUrl")]
		string ServerUrl { get; }

		// -(instancetype _Nonnull)initWithSdp:(NSString * _Nonnull)sdp sdpMLineIndex:(int)sdpMLineIndex sdpMid:(NSString * _Nullable)sdpMid __attribute__((objc_designated_initializer));
		[Export ("initWithSdp:sdpMLineIndex:sdpMid:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string sdp, int sdpMLineIndex, [NullAllowed] string sdpMid);
	}

	// @interface RTCIceCandidateErrorEvent : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCIceCandidateErrorEvent
	{
		// @property (readonly, nonatomic) NSString * _Nonnull address;
		[Export ("address")]
		string Address { get; }

		// @property (readonly, nonatomic) int port;
		[Export ("port")]
		int Port { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull url;
		[Export ("url")]
		string Url { get; }

		// @property (readonly, nonatomic) int errorCode;
		[Export ("errorCode")]
		int ErrorCode { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull errorText;
		[Export ("errorText")]
		string ErrorText { get; }
	}

	// @interface RTCIceServer : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCIceServer
	{
		// @property (readonly, nonatomic) NSArray<NSString *> * _Nonnull urlStrings;
		[Export ("urlStrings")]
		string[] UrlStrings { get; }

		// @property (readonly, nonatomic) NSString * _Nullable username;
		[NullAllowed, Export ("username")]
		string Username { get; }

		// @property (readonly, nonatomic) NSString * _Nullable credential;
		[NullAllowed, Export ("credential")]
		string Credential { get; }

		// @property (readonly, nonatomic) RTCTlsCertPolicy tlsCertPolicy;
		[Export ("tlsCertPolicy")]
		RTCTlsCertPolicy TlsCertPolicy { get; }

		// @property (readonly, nonatomic) NSString * _Nullable hostname;
		[NullAllowed, Export ("hostname")]
		string Hostname { get; }

		// @property (readonly, nonatomic) NSArray<NSString *> * _Nonnull tlsAlpnProtocols;
		[Export ("tlsAlpnProtocols")]
		string[] TlsAlpnProtocols { get; }

		// @property (readonly, nonatomic) NSArray<NSString *> * _Nonnull tlsEllipticCurves;
		[Export ("tlsEllipticCurves")]
		string[] TlsEllipticCurves { get; }

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings;
		[Export ("initWithURLStrings:")]
		NativeHandle Constructor (string[] urlStrings);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential;
		[Export ("initWithURLStrings:username:credential:")]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential tlsCertPolicy:(RTCTlsCertPolicy)tlsCertPolicy;
		[Export ("initWithURLStrings:username:credential:tlsCertPolicy:")]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential, RTCTlsCertPolicy tlsCertPolicy);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential tlsCertPolicy:(RTCTlsCertPolicy)tlsCertPolicy hostname:(NSString * _Nullable)hostname;
		[Export ("initWithURLStrings:username:credential:tlsCertPolicy:hostname:")]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential, RTCTlsCertPolicy tlsCertPolicy, [NullAllowed] string hostname);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential tlsCertPolicy:(RTCTlsCertPolicy)tlsCertPolicy hostname:(NSString * _Nullable)hostname tlsAlpnProtocols:(NSArray<NSString *> * _Nonnull)tlsAlpnProtocols;
		[Export ("initWithURLStrings:username:credential:tlsCertPolicy:hostname:tlsAlpnProtocols:")]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential, RTCTlsCertPolicy tlsCertPolicy, [NullAllowed] string hostname, string[] tlsAlpnProtocols);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential tlsCertPolicy:(RTCTlsCertPolicy)tlsCertPolicy hostname:(NSString * _Nullable)hostname tlsAlpnProtocols:(NSArray<NSString *> * _Nullable)tlsAlpnProtocols tlsEllipticCurves:(NSArray<NSString *> * _Nullable)tlsEllipticCurves __attribute__((objc_designated_initializer));
		[Export ("initWithURLStrings:username:credential:tlsCertPolicy:hostname:tlsAlpnProtocols:tlsEllipticCurves:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential, RTCTlsCertPolicy tlsCertPolicy, [NullAllowed] string hostname, [NullAllowed] string[] tlsAlpnProtocols, [NullAllowed] string[] tlsEllipticCurves);
	}

	// @interface RTCLegacyStatsReport : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCLegacyStatsReport
		: INativeObject
	{
		// @property (readonly, nonatomic) CFTimeInterval timestamp;
		[Export ("timestamp")]
		double Timestamp { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull type;
		[Export ("type")]
		string Type { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull reportId;
		[Export ("reportId")]
		string ReportId { get; }

		// @property (readonly, nonatomic) NSDictionary<NSString *,NSString *> * _Nonnull values;
		[Export ("values")]
		NSDictionary<NSString, NSString> Values { get; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const _Nonnull kRTCMediaConstraintsAudioNetworkAdaptorConfig;
		[Field ("kRTCMediaConstraintsAudioNetworkAdaptorConfig", "__Internal")]
		NSString kRTCMediaConstraintsAudioNetworkAdaptorConfig { get; }

		// extern NSString *const _Nonnull kRTCMediaConstraintsIceRestart;
		[Field ("kRTCMediaConstraintsIceRestart", "__Internal")]
		NSString kRTCMediaConstraintsIceRestart { get; }

		// extern NSString *const _Nonnull kRTCMediaConstraintsOfferToReceiveAudio;
		[Field ("kRTCMediaConstraintsOfferToReceiveAudio", "__Internal")]
		NSString kRTCMediaConstraintsOfferToReceiveAudio { get; }

		// extern NSString *const _Nonnull kRTCMediaConstraintsOfferToReceiveVideo;
		[Field ("kRTCMediaConstraintsOfferToReceiveVideo", "__Internal")]
		NSString kRTCMediaConstraintsOfferToReceiveVideo { get; }

		// extern NSString *const _Nonnull kRTCMediaConstraintsVoiceActivityDetection;
		[Field ("kRTCMediaConstraintsVoiceActivityDetection", "__Internal")]
		NSString kRTCMediaConstraintsVoiceActivityDetection { get; }

		// extern NSString *const _Nonnull kRTCMediaConstraintsValueTrue;
		[Field ("kRTCMediaConstraintsValueTrue", "__Internal")]
		NSString kRTCMediaConstraintsValueTrue { get; }

		// extern NSString *const _Nonnull kRTCMediaConstraintsValueFalse;
		[Field ("kRTCMediaConstraintsValueFalse", "__Internal")]
		NSString kRTCMediaConstraintsValueFalse { get; }
	}

	// @interface RTCMediaConstraints : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCMediaConstraints
	{
		// -(instancetype _Nonnull)initWithMandatoryConstraints:(NSDictionary<NSString *,NSString *> * _Nullable)mandatory optionalConstraints:(NSDictionary<NSString *,NSString *> * _Nullable)optional __attribute__((objc_designated_initializer));
		[Export ("initWithMandatoryConstraints:optionalConstraints:")]
		[DesignatedInitializer]
		NativeHandle Constructor ([NullAllowed] NSDictionary<NSString, NSString> mandatory, [NullAllowed] NSDictionary<NSString, NSString> optional);
	}

	// @interface RTCMediaStream : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCMediaStream
	{
		// @property (readonly, nonatomic, strong) NSArray<RTCAudioTrack *> * _Nonnull audioTracks;
		[Export ("audioTracks", ArgumentSemantic.Strong)]
		RTCAudioTrack[] AudioTracks { get; }

		// @property (readonly, nonatomic, strong) NSArray<RTCVideoTrack *> * _Nonnull videoTracks;
		[Export ("videoTracks", ArgumentSemantic.Strong)]
		RTCVideoTrack[] VideoTracks { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull streamId;
		[Export ("streamId")]
		string StreamId { get; }

		// -(void)addAudioTrack:(RTCAudioTrack * _Nonnull)audioTrack;
		[Export ("addAudioTrack:")]
		void AddAudioTrack (RTCAudioTrack audioTrack);

		// -(void)addVideoTrack:(RTCVideoTrack * _Nonnull)videoTrack;
		[Export ("addVideoTrack:")]
		void AddVideoTrack (RTCVideoTrack videoTrack);

		// -(void)removeAudioTrack:(RTCAudioTrack * _Nonnull)audioTrack;
		[Export ("removeAudioTrack:")]
		void RemoveAudioTrack (RTCAudioTrack audioTrack);

		// -(void)removeVideoTrack:(RTCVideoTrack * _Nonnull)videoTrack;
		[Export ("removeVideoTrack:")]
		void RemoveVideoTrack (RTCVideoTrack videoTrack);
	}

	// @interface RTCMetricsSampleInfo : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCMetricsSampleInfo
	{
		// @property (readonly, nonatomic) NSString * _Nonnull name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, nonatomic) int min;
		[Export ("min")]
		int Min { get; }

		// @property (readonly, nonatomic) int max;
		[Export ("max")]
		int Max { get; }

		// @property (readonly, nonatomic) int bucketCount;
		[Export ("bucketCount")]
		int BucketCount { get; }

		// @property (readonly, nonatomic) NSDictionary<NSNumber *,NSNumber *> * _Nonnull samples;
		[Export ("samples")]
		NSDictionary<NSNumber, NSNumber> Samples { get; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const _Nonnull kRTCPeerConnectionErrorDomain;
		[Field ("kRTCPeerConnectionErrorDomain", "__Internal")]
		NSString kRTCPeerConnectionErrorDomain { get; }

		// extern const int kRTCSessionDescriptionErrorCode;
		[Field ("kRTCSessionDescriptionErrorCode", "__Internal")]
		int kRTCSessionDescriptionErrorCode { get; }
	}

	// typedef void (^RTCCreateSessionDescriptionCompletionHandler)(RTCSessionDescription * _Nullable, NSError * _Nullable);
	delegate void RTCCreateSessionDescriptionCompletionHandler ([NullAllowed] RTCSessionDescription arg0, [NullAllowed] NSError arg1);

	// typedef void (^RTCSetSessionDescriptionCompletionHandler)(NSError * _Nullable);
	delegate void RTCSetSessionDescriptionCompletionHandler ([NullAllowed] NSError arg0);

	// @protocol RTCPeerConnectionDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface RTCPeerConnectionDelegate
	{
		// @required -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didChangeSignalingState:(RTCSignalingState)stateChanged;
		[Abstract]
		[Export ("peerConnection:didChangeSignalingState:")]
		void DidChangeSignalingState(RTCPeerConnection peerConnection, RTCSignalingState stateChanged);

		// @required -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didAddStream:(RTCMediaStream * _Nonnull)stream;
		[Abstract]
		[Export ("peerConnection:didAddStream:")]
		void DidAddStream(RTCPeerConnection peerConnection, RTCMediaStream stream);

		// @required -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didRemoveStream:(RTCMediaStream * _Nonnull)stream;
		[Abstract]
		[Export ("peerConnection:didRemoveStream:")]
		void DidRemoveStream(RTCPeerConnection peerConnection, RTCMediaStream stream);

		// @required -(void)peerConnectionShouldNegotiate:(RTCPeerConnection * _Nonnull)peerConnection;
		[Abstract]
		[Export ("peerConnectionShouldNegotiate:")]
		void ShouldNegotiate (RTCPeerConnection peerConnection);

		// @required -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didChangeIceConnectionState:(RTCIceConnectionState)newState;
		[Abstract]
		[Export ("peerConnection:didChangeIceConnectionState:")]
		void DidChangeIceConnectionState(RTCPeerConnection peerConnection, RTCIceConnectionState newState);

		// @required -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didChangeIceGatheringState:(RTCIceGatheringState)newState;
		[Abstract]
		[Export ("peerConnection:didChangeIceGatheringState:")]
		void DidChangeIceGatheringState(RTCPeerConnection peerConnection, RTCIceGatheringState newState);

		// @required -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didGenerateIceCandidate:(RTCIceCandidate * _Nonnull)candidate;
		[Abstract]
		[Export ("peerConnection:didGenerateIceCandidate:")]
		void DidGenerateIceCandidate(RTCPeerConnection peerConnection, RTCIceCandidate candidate);

		// @required -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didRemoveIceCandidates:(NSArray<RTCIceCandidate *> * _Nonnull)candidates;
		[Abstract]
		[Export ("peerConnection:didRemoveIceCandidates:")]
		void DidRemoveIceCandidates(RTCPeerConnection peerConnection, RTCIceCandidate[] candidates);

		// @required -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didOpenDataChannel:(RTCDataChannel * _Nonnull)dataChannel;
		[Abstract]
		[Export ("peerConnection:didOpenDataChannel:")]
		void DidOpenDataChannel(RTCPeerConnection peerConnection, RTCDataChannel dataChannel);

		// @optional -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didChangeStandardizedIceConnectionState:(RTCIceConnectionState)newState;
		[Export ("peerConnection:didChangeStandardizedIceConnectionState:")]
		void DidChangeStandardizedIceConnectionState(RTCPeerConnection peerConnection, RTCIceConnectionState newState);

		// @optional -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didChangeConnectionState:(RTCPeerConnectionState)newState;
		[Abstract]		
		[Export ("peerConnection:didChangeConnectionState:")]
		void DidChangeConnectionState(RTCPeerConnection peerConnection, RTCPeerConnectionState newState);

		// @optional -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didStartReceivingOnTransceiver:(RTCRtpTransceiver * _Nonnull)transceiver;
		[Export ("peerConnection:didStartReceivingOnTransceiver:")]
		void DidStartReceivingOnTransceiver(RTCPeerConnection peerConnection, RTCRtpTransceiver transceiver);

		// @optional -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didAddReceiver:(RTCRtpReceiver * _Nonnull)rtpReceiver streams:(NSArray<RTCMediaStream *> * _Nonnull)mediaStreams;
		[Export ("peerConnection:didAddReceiver:streams:")]
		void DidAddReceiver(RTCPeerConnection peerConnection, RTCRtpReceiver rtpReceiver, RTCMediaStream[] mediaStreams);

		// @optional -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didRemoveReceiver:(RTCRtpReceiver * _Nonnull)rtpReceiver;
		[Export ("peerConnection:didRemoveReceiver:")]
		void DidRemoveReceiver(RTCPeerConnection peerConnection, RTCRtpReceiver rtpReceiver);

		// @optional -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didChangeLocalCandidate:(RTCIceCandidate * _Nonnull)local remoteCandidate:(RTCIceCandidate * _Nonnull)remote lastReceivedMs:(int)lastDataReceivedMs changeReason:(NSString * _Nonnull)reason;
		[Export ("peerConnection:didChangeLocalCandidate:remoteCandidate:lastReceivedMs:changeReason:")]
		void DidChangeLocalCandidate(RTCPeerConnection peerConnection, RTCIceCandidate local, RTCIceCandidate remote, int lastDataReceivedMs, string reason);

		// @optional -(void)peerConnection:(RTCPeerConnection * _Nonnull)peerConnection didFailToGatherIceCandidate:(RTCIceCandidateErrorEvent * _Nonnull)event;
		[Export ("peerConnection:didFailToGatherIceCandidate:")]
		void DidFailToGatherIceCandidate(RTCPeerConnection peerConnection, RTCIceCandidateErrorEvent @event);
	}

	// @interface RTCPeerConnection : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCPeerConnection
	{
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		IRTCPeerConnectionDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<RTCPeerConnectionDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// @property (readonly, nonatomic) NSArray<RTCMediaStream *> * _Nonnull localStreams;
		[Export ("localStreams")]
		RTCMediaStream[] LocalStreams { get; }

		// @property (readonly, nonatomic) RTCSessionDescription * _Nullable localDescription;
		[NullAllowed, Export ("localDescription")]
		RTCSessionDescription LocalDescription { get; }

		// @property (readonly, nonatomic) RTCSessionDescription * _Nullable remoteDescription;
		[NullAllowed, Export ("remoteDescription")]
		RTCSessionDescription RemoteDescription { get; }

		// @property (readonly, nonatomic) RTCSignalingState signalingState;
		[Export ("signalingState")]
		RTCSignalingState SignalingState { get; }

		// @property (readonly, nonatomic) RTCIceConnectionState iceConnectionState;
		[Export ("iceConnectionState")]
		RTCIceConnectionState IceConnectionState { get; }

		// @property (readonly, nonatomic) RTCPeerConnectionState connectionState;
		[Export ("connectionState")]
		RTCPeerConnectionState ConnectionState { get; }

		// @property (readonly, nonatomic) RTCIceGatheringState iceGatheringState;
		[Export ("iceGatheringState")]
		RTCIceGatheringState IceGatheringState { get; }

		// @property (readonly, copy, nonatomic) RTCConfiguration * _Nonnull configuration;
		[Export ("configuration", ArgumentSemantic.Copy)]
		RTCConfiguration Configuration { get; }

		// @property (readonly, nonatomic) NSArray<RTCRtpSender *> * _Nonnull senders;
		[Export ("senders")]
		RTCRtpSender[] Senders { get; }

		// @property (readonly, nonatomic) NSArray<RTCRtpReceiver *> * _Nonnull receivers;
		[Export ("receivers")]
		RTCRtpReceiver[] Receivers { get; }

		// @property (readonly, nonatomic) NSArray<RTCRtpTransceiver *> * _Nonnull transceivers;
		[Export ("transceivers")]
		RTCRtpTransceiver[] Transceivers { get; }

		// -(BOOL)setConfiguration:(RTCConfiguration * _Nonnull)configuration;
		[Export ("setConfiguration:")]
		bool SetConfiguration (RTCConfiguration configuration);

		// -(void)close;
		[Export ("close")]
		void Close ();

		// -(void)addIceCandidate:(RTCIceCandidate * _Nonnull)candidate __attribute__((deprecated("Please use addIceCandidate:completionHandler: instead")));
		[Export ("addIceCandidate:")]
		void AddIceCandidate (RTCIceCandidate candidate);

		// -(void)addIceCandidate:(RTCIceCandidate * _Nonnull)candidate completionHandler:(void (^ _Nonnull)(NSError * _Nullable))completionHandler;
		[Export ("addIceCandidate:completionHandler:")]
		void AddIceCandidate (RTCIceCandidate candidate, Action<NSError> completionHandler);

		// -(void)removeIceCandidates:(NSArray<RTCIceCandidate *> * _Nonnull)candidates;
		[Export ("removeIceCandidates:")]
		void RemoveIceCandidates (RTCIceCandidate[] candidates);

		// -(void)addStream:(RTCMediaStream * _Nonnull)stream;
		[Export ("addStream:")]
		void AddStream (RTCMediaStream stream);

		// -(void)removeStream:(RTCMediaStream * _Nonnull)stream;
		[Export ("removeStream:")]
		void RemoveStream (RTCMediaStream stream);

		// -(RTCRtpSender * _Nullable)addTrack:(RTCMediaStreamTrack * _Nonnull)track streamIds:(NSArray<NSString *> * _Nonnull)streamIds;
		[Export ("addTrack:streamIds:")]
		[return: NullAllowed]
		RTCRtpSender AddTrack (RTCMediaStreamTrack track, string[] streamIds);

		// -(BOOL)removeTrack:(RTCRtpSender * _Nonnull)sender;
		[Export ("removeTrack:")]
		bool RemoveTrack (RTCRtpSender sender);

		// -(RTCRtpTransceiver * _Nullable)addTransceiverWithTrack:(RTCMediaStreamTrack * _Nonnull)track;
		[Export ("addTransceiverWithTrack:")]
		[return: NullAllowed]
		RTCRtpTransceiver AddTransceiverWithTrack (RTCMediaStreamTrack track);

		// -(RTCRtpTransceiver * _Nullable)addTransceiverWithTrack:(RTCMediaStreamTrack * _Nonnull)track init:(RTCRtpTransceiverInit * _Nonnull)init;
		[Export ("addTransceiverWithTrack:init:")]
		[return: NullAllowed]
		RTCRtpTransceiver AddTransceiverWithTrack (RTCMediaStreamTrack track, RTCRtpTransceiverInit init);

		// -(RTCRtpTransceiver * _Nullable)addTransceiverOfType:(RTCRtpMediaType)mediaType;
		[Export ("addTransceiverOfType:")]
		[return: NullAllowed]
		RTCRtpTransceiver AddTransceiverOfType (RTCRtpMediaType mediaType);

		// -(RTCRtpTransceiver * _Nullable)addTransceiverOfType:(RTCRtpMediaType)mediaType init:(RTCRtpTransceiverInit * _Nonnull)init;
		[Export ("addTransceiverOfType:init:")]
		[return: NullAllowed]
		RTCRtpTransceiver AddTransceiverOfType (RTCRtpMediaType mediaType, RTCRtpTransceiverInit init);

		// -(void)restartIce;
		[Export ("restartIce")]
		void RestartIce ();

		// -(void)offerForConstraints:(RTCMediaConstraints * _Nonnull)constraints completionHandler:(RTCCreateSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("offerForConstraints:completionHandler:")]
		void OfferForConstraints (RTCMediaConstraints constraints, RTCCreateSessionDescriptionCompletionHandler completionHandler);

		// -(void)answerForConstraints:(RTCMediaConstraints * _Nonnull)constraints completionHandler:(RTCCreateSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("answerForConstraints:completionHandler:")]
		void AnswerForConstraints (RTCMediaConstraints constraints, RTCCreateSessionDescriptionCompletionHandler completionHandler);

		// -(void)setLocalDescription:(RTCSessionDescription * _Nonnull)sdp completionHandler:(RTCSetSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("setLocalDescription:completionHandler:")]
		void SetLocalDescription (RTCSessionDescription sdp, RTCSetSessionDescriptionCompletionHandler completionHandler);

		// -(void)setLocalDescriptionWithCompletionHandler:(RTCSetSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("setLocalDescriptionWithCompletionHandler:")]
		void SetLocalDescriptionWithCompletionHandler (RTCSetSessionDescriptionCompletionHandler completionHandler);

		// -(void)setRemoteDescription:(RTCSessionDescription * _Nonnull)sdp completionHandler:(RTCSetSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("setRemoteDescription:completionHandler:")]
		void SetRemoteDescription (RTCSessionDescription sdp, RTCSetSessionDescriptionCompletionHandler completionHandler);

		// -(BOOL)setBweMinBitrateBps:(NSNumber * _Nullable)minBitrateBps currentBitrateBps:(NSNumber * _Nullable)currentBitrateBps maxBitrateBps:(NSNumber * _Nullable)maxBitrateBps;
		[Export ("setBweMinBitrateBps:currentBitrateBps:maxBitrateBps:")]
		bool SetBweMinBitrateBps ([NullAllowed] NSNumber minBitrateBps, [NullAllowed] NSNumber currentBitrateBps, [NullAllowed] NSNumber maxBitrateBps);

		// -(BOOL)startRtcEventLogWithFilePath:(NSString * _Nonnull)filePath maxSizeInBytes:(int64_t)maxSizeInBytes;
		[Export ("startRtcEventLogWithFilePath:maxSizeInBytes:")]
		bool StartRtcEventLogWithFilePath (string filePath, long maxSizeInBytes);

		// -(void)stopRtcEventLog;
		[Export ("stopRtcEventLog")]
		void StopRtcEventLog ();
	}

	// @interface Media (RTCPeerConnection)
	[Category]
	[BaseType (typeof(RTCPeerConnection))]
	interface RTCPeerConnection_Media
	{
		// -(RTCRtpSender * _Nonnull)senderWithKind:(NSString * _Nonnull)kind streamId:(NSString * _Nonnull)streamId;
		[Export ("senderWithKind:streamId:")]
		RTCRtpSender SenderWithKind (string kind, string streamId);
	}

	// @interface DataChannel (RTCPeerConnection)
	[Category]
	[BaseType (typeof(RTCPeerConnection))]
	interface RTCPeerConnection_DataChannel
	{
		// -(RTCDataChannel * _Nullable)dataChannelForLabel:(NSString * _Nonnull)label configuration:(RTCDataChannelConfiguration * _Nonnull)configuration;
		[Export ("dataChannelForLabel:configuration:")]
		[return: NullAllowed]
		RTCDataChannel DataChannelForLabel (string label, RTCDataChannelConfiguration configuration);
	}

	// typedef void (^RTCStatisticsCompletionHandler)(RTCStatisticsReport * _Nonnull);
	delegate void RTCStatisticsCompletionHandler (RTCStatisticsReport arg0);

	// @interface Stats (RTCPeerConnection)
	[Category]
	[BaseType (typeof(RTCPeerConnection))]
	interface RTCPeerConnection_Stats
	{
		// -(void)statsForTrack:(RTCMediaStreamTrack * _Nullable)mediaStreamTrack statsOutputLevel:(RTCStatsOutputLevel)statsOutputLevel completionHandler:(void (^ _Nullable)(NSArray<RTCLegacyStatsReport *> * _Nonnull))completionHandler;
		[Export ("statsForTrack:statsOutputLevel:completionHandler:")]
		void StatsForTrack ([NullAllowed] RTCMediaStreamTrack mediaStreamTrack, RTCStatsOutputLevel statsOutputLevel, [NullAllowed] Action<NSArray<RTCLegacyStatsReport>> completionHandler);

		// -(void)statisticsWithCompletionHandler:(RTCStatisticsCompletionHandler _Nonnull)completionHandler;
		[Export ("statisticsWithCompletionHandler:")]
		void StatisticsWithCompletionHandler (RTCStatisticsCompletionHandler completionHandler);

		// -(void)statisticsForSender:(RTCRtpSender * _Nonnull)sender completionHandler:(RTCStatisticsCompletionHandler _Nonnull)completionHandler;
		[Export ("statisticsForSender:completionHandler:")]
		void StatisticsForSender (RTCRtpSender sender, RTCStatisticsCompletionHandler completionHandler);

		// -(void)statisticsForReceiver:(RTCRtpReceiver * _Nonnull)receiver completionHandler:(RTCStatisticsCompletionHandler _Nonnull)completionHandler;
		[Export ("statisticsForReceiver:completionHandler:")]
		void StatisticsForReceiver (RTCRtpReceiver receiver, RTCStatisticsCompletionHandler completionHandler);
	}

	// @interface RTCPeerConnectionFactory : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCPeerConnectionFactory
	{
		// -(instancetype _Nonnull)initWithEncoderFactory:(id<RTCVideoEncoderFactory> _Nullable)encoderFactory decoderFactory:(id<RTCVideoDecoderFactory> _Nullable)decoderFactory;
		[Export ("initWithEncoderFactory:decoderFactory:")]
		NativeHandle Constructor ([NullAllowed] RTCVideoEncoderFactory encoderFactory, [NullAllowed] RTCVideoDecoderFactory decoderFactory);

		// -(instancetype _Nonnull)initWithEncoderFactory:(id<RTCVideoEncoderFactory> _Nullable)encoderFactory decoderFactory:(id<RTCVideoDecoderFactory> _Nullable)decoderFactory audioDevice:(id<RTCAudioDevice> _Nullable)audioDevice;
		[Export ("initWithEncoderFactory:decoderFactory:audioDevice:")]
		NativeHandle Constructor ([NullAllowed] RTCVideoEncoderFactory encoderFactory, [NullAllowed] RTCVideoDecoderFactory decoderFactory, [NullAllowed] RTCAudioDevice audioDevice);

		// -(RTCAudioSource * _Nonnull)audioSourceWithConstraints:(RTCMediaConstraints * _Nullable)constraints;
		[Export ("audioSourceWithConstraints:")]
		RTCAudioSource AudioSourceWithConstraints ([NullAllowed] RTCMediaConstraints constraints);

		// -(RTCAudioTrack * _Nonnull)audioTrackWithTrackId:(NSString * _Nonnull)trackId;
		[Export ("audioTrackWithTrackId:")]
		RTCAudioTrack AudioTrackWithTrackId (string trackId);

		// -(RTCAudioTrack * _Nonnull)audioTrackWithSource:(RTCAudioSource * _Nonnull)source trackId:(NSString * _Nonnull)trackId;
		[Export ("audioTrackWithSource:trackId:")]
		RTCAudioTrack AudioTrackWithSource (RTCAudioSource source, string trackId);

		// -(RTCVideoSource * _Nonnull)videoSource;
		[Export ("videoSource")]
		////[Verify (MethodToProperty)]
		RTCVideoSource VideoSource { get; }

		// -(RTCVideoSource * _Nonnull)videoSourceForScreenCast:(BOOL)forScreenCast;
		[Export ("videoSourceForScreenCast:")]
		RTCVideoSource VideoSourceForScreenCast (bool forScreenCast);

		// -(RTCVideoTrack * _Nonnull)videoTrackWithSource:(RTCVideoSource * _Nonnull)source trackId:(NSString * _Nonnull)trackId;
		[Export ("videoTrackWithSource:trackId:")]
		RTCVideoTrack VideoTrackWithSource (RTCVideoSource source, string trackId);

		// -(RTCMediaStream * _Nonnull)mediaStreamWithStreamId:(NSString * _Nonnull)streamId;
		[Export ("mediaStreamWithStreamId:")]
		RTCMediaStream MediaStreamWithStreamId (string streamId);

		// -(RTCPeerConnection * _Nullable)peerConnectionWithConfiguration:(RTCConfiguration * _Nonnull)configuration constraints:(RTCMediaConstraints * _Nonnull)constraints delegate:(id<RTCPeerConnectionDelegate> _Nullable)delegate;
		[Export ("peerConnectionWithConfiguration:constraints:delegate:")]
		[return: NullAllowed]
		RTCPeerConnection PeerConnectionWithConfiguration (RTCConfiguration configuration, RTCMediaConstraints constraints, [NullAllowed] IRTCPeerConnectionDelegate @delegate);

		// -(RTCPeerConnection * _Nullable)peerConnectionWithConfiguration:(RTCConfiguration * _Nonnull)configuration constraints:(RTCMediaConstraints * _Nonnull)constraints certificateVerifier:(id<RTCSSLCertificateVerifier> _Nonnull)certificateVerifier delegate:(id<RTCPeerConnectionDelegate> _Nullable)delegate;
		[Export ("peerConnectionWithConfiguration:constraints:certificateVerifier:delegate:")]
		[return: NullAllowed]
		RTCPeerConnection PeerConnectionWithConfiguration (RTCConfiguration configuration, RTCMediaConstraints constraints, RTCSSLCertificateVerifier certificateVerifier, [NullAllowed] IRTCPeerConnectionDelegate @delegate);

		// -(void)setOptions:(RTCPeerConnectionFactoryOptions * _Nonnull)options;
		[Export ("setOptions:")]
		void SetOptions (RTCPeerConnectionFactoryOptions options);

		// -(BOOL)startAecDumpWithFilePath:(NSString * _Nonnull)filePath maxSizeInBytes:(int64_t)maxSizeInBytes;
		[Export ("startAecDumpWithFilePath:maxSizeInBytes:")]
		bool StartAecDumpWithFilePath (string filePath, long maxSizeInBytes);

		// -(void)stopAecDump;
		[Export ("stopAecDump")]
		void StopAecDump ();
	}

	// @interface RTCPeerConnectionFactoryOptions : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCPeerConnectionFactoryOptions
	{
		// @property (assign, nonatomic) BOOL disableEncryption;
		[Export ("disableEncryption")]
		bool DisableEncryption { get; set; }

		// @property (assign, nonatomic) BOOL disableNetworkMonitor;
		[Export ("disableNetworkMonitor")]
		bool DisableNetworkMonitor { get; set; }

		// @property (assign, nonatomic) BOOL ignoreLoopbackNetworkAdapter;
		[Export ("ignoreLoopbackNetworkAdapter")]
		bool IgnoreLoopbackNetworkAdapter { get; set; }

		// @property (assign, nonatomic) BOOL ignoreVPNNetworkAdapter;
		[Export ("ignoreVPNNetworkAdapter")]
		bool IgnoreVPNNetworkAdapter { get; set; }

		// @property (assign, nonatomic) BOOL ignoreCellularNetworkAdapter;
		[Export ("ignoreCellularNetworkAdapter")]
		bool IgnoreCellularNetworkAdapter { get; set; }

		// @property (assign, nonatomic) BOOL ignoreWiFiNetworkAdapter;
		[Export ("ignoreWiFiNetworkAdapter")]
		bool IgnoreWiFiNetworkAdapter { get; set; }

		// @property (assign, nonatomic) BOOL ignoreEthernetNetworkAdapter;
		[Export ("ignoreEthernetNetworkAdapter")]
		bool IgnoreEthernetNetworkAdapter { get; set; }
	}

	// @interface RTCRtcpParameters : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCRtcpParameters
	{
		// @property (readonly, copy, nonatomic) NSString * _Nonnull cname;
		[Export ("cname")]
		string Cname { get; }

		// @property (assign, nonatomic) BOOL isReducedSize;
		[Export ("isReducedSize")]
		bool IsReducedSize { get; set; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern const NSString *const _Nonnull kRTCRtxCodecName;
		[Field ("kRTCRtxCodecName", "__Internal")]
		NSString kRTCRtxCodecName { get; }

		// extern const NSString *const _Nonnull kRTCRedCodecName;
		[Field ("kRTCRedCodecName", "__Internal")]
		NSString kRTCRedCodecName { get; }

		// extern const NSString *const _Nonnull kRTCUlpfecCodecName;
		[Field ("kRTCUlpfecCodecName", "__Internal")]
		NSString kRTCUlpfecCodecName { get; }

		// extern const NSString *const _Nonnull kRTCFlexfecCodecName;
		[Field ("kRTCFlexfecCodecName", "__Internal")]
		NSString kRTCFlexfecCodecName { get; }

		// extern const NSString *const _Nonnull kRTCOpusCodecName;
		[Field ("kRTCOpusCodecName", "__Internal")]
		NSString kRTCOpusCodecName { get; }

		// extern const NSString *const _Nonnull kRTCIsacCodecName;
		[Field ("kRTCIsacCodecName", "__Internal")]
		NSString kRTCIsacCodecName { get; }

		// extern const NSString *const _Nonnull kRTCL16CodecName;
		[Field ("kRTCL16CodecName", "__Internal")]
		NSString kRTCL16CodecName { get; }

		// extern const NSString *const _Nonnull kRTCG722CodecName;
		[Field ("kRTCG722CodecName", "__Internal")]
		NSString kRTCG722CodecName { get; }

		// extern const NSString *const _Nonnull kRTCIlbcCodecName;
		[Field ("kRTCIlbcCodecName", "__Internal")]
		NSString kRTCIlbcCodecName { get; }

		// extern const NSString *const _Nonnull kRTCPcmuCodecName;
		[Field ("kRTCPcmuCodecName", "__Internal")]
		NSString kRTCPcmuCodecName { get; }

		// extern const NSString *const _Nonnull kRTCPcmaCodecName;
		[Field ("kRTCPcmaCodecName", "__Internal")]
		NSString kRTCPcmaCodecName { get; }

		// extern const NSString *const _Nonnull kRTCDtmfCodecName;
		[Field ("kRTCDtmfCodecName", "__Internal")]
		NSString kRTCDtmfCodecName { get; }

		// extern const NSString *const _Nonnull kRTCComfortNoiseCodecName;
		[Field ("kRTCComfortNoiseCodecName", "__Internal")]
		NSString kRTCComfortNoiseCodecName { get; }

		// extern const NSString *const _Nonnull kRTCVp8CodecName;
		[Field ("kRTCVp8CodecName", "__Internal")]
		NSString kRTCVp8CodecName { get; }

		// extern const NSString *const _Nonnull kRTCVp9CodecName;
		[Field ("kRTCVp9CodecName", "__Internal")]
		NSString kRTCVp9CodecName { get; }

		// extern const NSString *const _Nonnull kRTCH264CodecName;
		[Field ("kRTCH264CodecName", "__Internal")]
		NSString kRTCH264CodecName { get; }
	}

	// @interface RTCRtpCodecParameters : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCRtpCodecParameters
	{
		// @property (assign, nonatomic) int payloadType;
		[Export ("payloadType")]
		int PayloadType { get; set; }

		// @property (readonly, nonatomic) NSString * _Nonnull name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull kind;
		[Export ("kind")]
		string Kind { get; }

		// @property (readonly, nonatomic) NSNumber * _Nullable clockRate;
		[NullAllowed, Export ("clockRate")]
		NSNumber ClockRate { get; }

		// @property (readonly, nonatomic) NSNumber * _Nullable numChannels;
		[NullAllowed, Export ("numChannels")]
		NSNumber NumChannels { get; }

		// @property (readonly, nonatomic) NSDictionary * _Nonnull parameters;
		[Export ("parameters")]
		NSDictionary Parameters { get; }
	}

	// @interface RTCRtpEncodingParameters : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCRtpEncodingParameters
	{
		// @property (copy, nonatomic) NSString * _Nullable rid;
		[NullAllowed, Export ("rid")]
		string Rid { get; set; }

		// @property (assign, nonatomic) BOOL isActive;
		[Export ("isActive")]
		bool IsActive { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable maxBitrateBps;
		[NullAllowed, Export ("maxBitrateBps", ArgumentSemantic.Copy)]
		NSNumber MaxBitrateBps { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable minBitrateBps;
		[NullAllowed, Export ("minBitrateBps", ArgumentSemantic.Copy)]
		NSNumber MinBitrateBps { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable maxFramerate;
		[NullAllowed, Export ("maxFramerate", ArgumentSemantic.Copy)]
		NSNumber MaxFramerate { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable numTemporalLayers;
		[NullAllowed, Export ("numTemporalLayers", ArgumentSemantic.Copy)]
		NSNumber NumTemporalLayers { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable scaleResolutionDownBy;
		[NullAllowed, Export ("scaleResolutionDownBy", ArgumentSemantic.Copy)]
		NSNumber ScaleResolutionDownBy { get; set; }

		// @property (readonly, nonatomic) NSNumber * _Nullable ssrc;
		[NullAllowed, Export ("ssrc")]
		NSNumber Ssrc { get; }

		// @property (assign, nonatomic) double bitratePriority;
		[Export ("bitratePriority")]
		double BitratePriority { get; set; }

		// @property (assign, nonatomic) RTCPriority networkPriority;
		[Export ("networkPriority", ArgumentSemantic.Assign)]
		RTCPriority NetworkPriority { get; set; }

		// @property (assign, nonatomic) BOOL adaptiveAudioPacketTime;
		[Export ("adaptiveAudioPacketTime")]
		bool AdaptiveAudioPacketTime { get; set; }
	}

	// @interface RTCRtpHeaderExtension : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCRtpHeaderExtension
	{
		// @property (readonly, copy, nonatomic) NSString * _Nonnull uri;
		[Export ("uri")]
		string Uri { get; }

		// @property (readonly, nonatomic) int id;
		[Export ("id")]
		int Id { get; }

		// @property (readonly, getter = isEncrypted, nonatomic) BOOL encrypted;
		[Export ("encrypted")]
		bool Encrypted { [Bind ("isEncrypted")] get; }
	}

	// @interface RTCRtpParameters : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCRtpParameters
	{
		// @property (copy, nonatomic) NSString * _Nonnull transactionId;
		[Export ("transactionId")]
		string TransactionId { get; set; }

		// @property (readonly, copy, nonatomic) RTCRtcpParameters * _Nonnull rtcp;
		[Export ("rtcp", ArgumentSemantic.Copy)]
		RTCRtcpParameters Rtcp { get; }

		// @property (readonly, copy, nonatomic) NSArray<RTCRtpHeaderExtension *> * _Nonnull headerExtensions;
		[Export ("headerExtensions", ArgumentSemantic.Copy)]
		RTCRtpHeaderExtension[] HeaderExtensions { get; }

		// @property (copy, nonatomic) NSArray<RTCRtpEncodingParameters *> * _Nonnull encodings;
		[Export ("encodings", ArgumentSemantic.Copy)]
		RTCRtpEncodingParameters[] Encodings { get; set; }

		// @property (copy, nonatomic) NSArray<RTCRtpCodecParameters *> * _Nonnull codecs;
		[Export ("codecs", ArgumentSemantic.Copy)]
		RTCRtpCodecParameters[] Codecs { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable degradationPreference;
		[NullAllowed, Export ("degradationPreference", ArgumentSemantic.Copy)]
		NSNumber DegradationPreference { get; set; }
	}

	// @protocol RTCRtpReceiverDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface RTCRtpReceiverDelegate
	{
		// @required -(void)rtpReceiver:(RTCRtpReceiver * _Nonnull)rtpReceiver didReceiveFirstPacketForMediaType:(RTCRtpMediaType)mediaType;
		[Abstract]
		[Export ("rtpReceiver:didReceiveFirstPacketForMediaType:")]
		void DidReceiveFirstPacketForMediaType (RTCRtpReceiver rtpReceiver, RTCRtpMediaType mediaType);
	}

	// @protocol RTCRtpReceiver <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCRtpReceiver
    {
		// @required @property (readonly, nonatomic) NSString * _Nonnull receiverId;
		[Abstract]
		[Export ("receiverId")]
		string ReceiverId { get; }

		// @required @property (readonly, nonatomic) RTCRtpParameters * _Nonnull parameters;
		[Abstract]
		[Export ("parameters")]
		RTCRtpParameters Parameters { get; }

		// @required @property (readonly, nonatomic) RTCMediaStreamTrack * _Nullable track;
		[Abstract]
		[NullAllowed, Export ("track")]
		RTCMediaStreamTrack Track { get; }

		[Wrap ("WeakDelegate"), Abstract]
		[NullAllowed]
		IRTCRtpReceiverDelegate Delegate { get; set; }

		// @required @property (nonatomic, weak) id<RTCRtpReceiverDelegate> _Nullable delegate;
		[Abstract]
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }
    }

    // @interface RTCRtpReceiver : NSObject <RTCRtpReceiver>
    ////[BaseType (typeof(NSObject))]
    ////[DisableDefaultCtor]
    ////interface RTCRtpReceiver : IRTCRtpReceiver
    ////{
    ////}

    // @protocol RTCDtmfSender <NSObject>
    /*
    Check whether adding [Model] to this declaration is appropriate.
    [Model] is used to generate a C# class that implements this protocol,
    and might be useful for protocols that consumers are supposed to implement,
    since consumers can subclass the generated class instead of implementing
    the generated interface. If consumers are not supposed to implement this
    protocol, then [Model] is redundant and will generate code that will never
    be used.
    */
    [Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCDtmfSender
	{
		// @required @property (readonly, nonatomic) BOOL canInsertDtmf;
		[Abstract]
		[Export ("canInsertDtmf")]
		bool CanInsertDtmf { get; }

		// @required -(BOOL)insertDtmf:(NSString * _Nonnull)tones duration:(NSTimeInterval)duration interToneGap:(NSTimeInterval)interToneGap;
		[Abstract]
		[Export ("insertDtmf:duration:interToneGap:")]
		bool InsertDtmf(string tones, double duration, double interToneGap);

		// @required -(NSString * _Nonnull)remainingTones;
		[Abstract]
		[Export ("remainingTones")]
		////[Verify (MethodToProperty)]
		string RemainingTones { get; }

		// @required -(NSTimeInterval)duration;
		[Abstract]
		[Export ("duration")]
		////[Verify (MethodToProperty)]
		double Duration { get; }

		// @required -(NSTimeInterval)interToneGap;
		[Abstract]
		[Export ("interToneGap")]
		////[Verify (MethodToProperty)]
		double InterToneGap { get; }
	}

	// @protocol RTCRtpSender <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCRtpSender
    {
		// @required @property (readonly, nonatomic) NSString * _Nonnull senderId;
		[Abstract]
		[Export ("senderId")]
		string SenderId { get; }

		// @required @property (copy, nonatomic) RTCRtpParameters * _Nonnull parameters;
		[Abstract]
		[Export ("parameters", ArgumentSemantic.Copy)]
		RTCRtpParameters Parameters { get; set; }

		// @required @property (copy, nonatomic) RTCMediaStreamTrack * _Nullable track;
		[Abstract]
		[NullAllowed, Export ("track", ArgumentSemantic.Copy)]
		RTCMediaStreamTrack Track { get; set; }

		// @required @property (copy, nonatomic) NSArray<NSString *> * _Nonnull streamIds;
		[Abstract]
		[Export ("streamIds", ArgumentSemantic.Copy)]
		string[] StreamIds { get; set; }

		// @required @property (readonly, nonatomic) id<RTCDtmfSender> _Nullable dtmfSender;
		[Abstract]
		[NullAllowed, Export ("dtmfSender")]
		RTCDtmfSender DtmfSender { get; }
	}

    // @interface RTCRtpSender : NSObject <RTCRtpSender>
    ////[BaseType (typeof(NSObject))]
    ////[DisableDefaultCtor]
    ////interface RTCRtpSender : IRTCRtpSender
    ////{
    ////}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const _Nonnull kRTCRtpTransceiverErrorDomain;
		[Field ("kRTCRtpTransceiverErrorDomain", "__Internal")]
		NSString kRTCRtpTransceiverErrorDomain { get; }
	}

	// @interface RTCRtpTransceiverInit : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCRtpTransceiverInit
	{
		// @property (nonatomic) RTCRtpTransceiverDirection direction;
		[Export ("direction", ArgumentSemantic.Assign)]
		RTCRtpTransceiverDirection Direction { get; set; }

		// @property (nonatomic) NSArray<NSString *> * _Nonnull streamIds;
		[Export ("streamIds", ArgumentSemantic.Assign)]
		string[] StreamIds { get; set; }

		// @property (nonatomic) NSArray<RTCRtpEncodingParameters *> * _Nonnull sendEncodings;
		[Export ("sendEncodings", ArgumentSemantic.Assign)]
		RTCRtpEncodingParameters[] SendEncodings { get; set; }
	}

	// @protocol RTCRtpTransceiver <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface RTCRtpTransceiver
    {
		// @required @property (readonly, nonatomic) RTCRtpMediaType mediaType;
		[Abstract]
		[Export ("mediaType")]
		RTCRtpMediaType MediaType { get; }

		// @required @property (readonly, nonatomic) NSString * _Nonnull mid;
		[Abstract]
		[Export ("mid")]
		string Mid { get; }

		// @required @property (readonly, nonatomic) RTCRtpSender * _Nonnull sender;
		[Abstract]
		[Export ("sender")]
		RTCRtpSender Sender { get; }

		// @required @property (readonly, nonatomic) RTCRtpReceiver * _Nonnull receiver;
		[Abstract]
		[Export ("receiver")]
		RTCRtpReceiver Receiver { get; }

		// @required @property (readonly, nonatomic) BOOL isStopped;
		[Abstract]
		[Export ("isStopped")]
		bool IsStopped { get; }

		// @required @property (readonly, nonatomic) RTCRtpTransceiverDirection direction;
		[Abstract]
		[Export ("direction")]
		RTCRtpTransceiverDirection Direction { get; }

		// @required -(BOOL)currentDirection:(RTCRtpTransceiverDirection * _Nonnull)currentDirectionOut;
		[Abstract]
		[Export ("currentDirection:")]
		/****unsafe****/ bool CurrentDirection (/****RTCRtpTransceiverDirection* currentDirectionOut****/ref RTCRtpTransceiverDirection currentDirectionOut);

		// @required -(void)stopInternal;
		[Abstract]
		[Export ("stopInternal")]
		void StopInternal ();

		// @required -(void)setDirection:(RTCRtpTransceiverDirection)direction error:(NSError * _Nullable * _Nullable)error;
		[Abstract]
		[Export ("setDirection:error:")]
		void SetDirection (RTCRtpTransceiverDirection direction, [NullAllowed] out NSError error);
	}

	// @interface RTCRtpTransceiver : NSObject <RTCRtpTransceiver>
	////[BaseType (typeof(NSObject))]
	////[DisableDefaultCtor]
	////interface RTCRtpTransceiver : IRTCRtpTransceiver
	////{
	////}

	// @interface RTCSessionDescription : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCSessionDescription
	{
		// @property (readonly, nonatomic) RTCSdpType type;
		[Export ("type")]
		RTCSdpType Type { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull sdp;
		[Export ("sdp")]
		string Sdp { get; }

		// -(instancetype _Nonnull)initWithType:(RTCSdpType)type sdp:(NSString * _Nonnull)sdp __attribute__((objc_designated_initializer));
		[Export ("initWithType:sdp:")]
		[DesignatedInitializer]
		NativeHandle Constructor (RTCSdpType type, string sdp);

		// +(NSString * _Nonnull)stringForType:(RTCSdpType)type;
		[Static]
		[Export ("stringForType:")]
		string StringForType (RTCSdpType type);

		// +(RTCSdpType)typeForString:(NSString * _Nonnull)string;
		[Static]
		[Export ("typeForString:")]
		RTCSdpType TypeForString (string @string);
	}

	// @interface RTCStatisticsReport : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCStatisticsReport
	{
		// @property (readonly, nonatomic) CFTimeInterval timestamp_us;
		[Export ("timestamp_us")]
		double Timestamp_us { get; }

		// @property (readonly, nonatomic) NSDictionary<NSString *,RTCStatistics *> * _Nonnull statistics;
		[Export ("statistics")]
		NSDictionary<NSString, RTCStatistics> Statistics { get; }
	}

	// @interface RTCStatistics : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface RTCStatistics
		: INativeObject
	{
		// @property (readonly, nonatomic) NSString * _Nonnull id;
		[Export ("id")]
		string Id { get; }

		// @property (readonly, nonatomic) CFTimeInterval timestamp_us;
		[Export ("timestamp_us")]
		double Timestamp_us { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull type;
		[Export ("type")]
		string Type { get; }

		// @property (readonly, nonatomic) NSDictionary<NSString *,NSObject *> * _Nonnull values;
		[Export ("values")]
		NSDictionary<NSString, NSObject> Values { get; }
	}

    // @interface RTCVideoSource : RTCMediaSource <RTCVideoCapturerDelegate>
[Protocol]
    [BaseType (typeof(RTCMediaSource))]
	[DisableDefaultCtor]
	interface RTCVideoSource : /****I***/RTCVideoCapturerDelegate
	{
		// -(void)adaptOutputFormatToWidth:(int)width height:(int)height fps:(int)fps;
		[Export ("adaptOutputFormatToWidth:height:fps:")]
		void AdaptOutputFormatToWidth (int width, int height, int fps);
	}

	// @interface RTCVideoTrack : RTCMediaStreamTrack
	[BaseType (typeof(RTCMediaStreamTrack))]
	[DisableDefaultCtor]
	interface RTCVideoTrack
	{
		// @property (readonly, nonatomic) RTCVideoSource * _Nonnull source;
		[Export ("source")]
		RTCVideoSource Source { get; }

		// -(void)addRenderer:(id<RTCVideoRenderer> _Nonnull)renderer;
		[Export ("addRenderer:")]
		void AddRenderer (IRTCVideoRenderer renderer);

		// -(void)removeRenderer:(id<RTCVideoRenderer> _Nonnull)renderer;
		[Export ("removeRenderer:")]
		void RemoveRenderer (IRTCVideoRenderer renderer);
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const kRTCVideoCodecVp8Name;
		[Field ("kRTCVideoCodecVp8Name", "__Internal")]
		NSString kRTCVideoCodecVp8Name { get; }

		// extern NSString *const kRTCVideoCodecVp9Name;
		[Field ("kRTCVideoCodecVp9Name", "__Internal")]
		NSString kRTCVideoCodecVp9Name { get; }

		// extern NSString *const kRTCVideoCodecAv1Name;
		[Field ("kRTCVideoCodecAv1Name", "__Internal")]
		NSString kRTCVideoCodecAv1Name { get; }
	}

	// @interface RTCVideoDecoderVP8 : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoDecoderVP8
	{
		// +(id<RTCVideoDecoder>)vp8Decoder;
		[Static]
		[Export ("vp8Decoder")]
		////[Verify (MethodToProperty)]
		RTCVideoDecoder Vp8Decoder { get; }
	}

	// @interface RTCVideoDecoderVP9 : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoDecoderVP9
	{
		// +(id<RTCVideoDecoder>)vp9Decoder;
		[Static]
		[Export ("vp9Decoder")]
		////[Verify (MethodToProperty)]
		RTCVideoDecoder Vp9Decoder { get; }

		// +(_Bool)isSupported;
		[Static]
		[Export ("isSupported")]
		////[Verify (MethodToProperty)]
		bool IsSupported { get; }
	}

	// @interface RTCVideoDecoderAV1 : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoDecoderAV1
	{
		// +(id<RTCVideoDecoder>)av1Decoder;
		[Static]
		[Export ("av1Decoder")]
		////[Verify (MethodToProperty)]
		RTCVideoDecoder Av1Decoder { get; }
	}

	// @interface RTCVideoEncoderVP8 : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderVP8
	{
		// +(id<RTCVideoEncoder>)vp8Encoder;
		[Static]
		[Export ("vp8Encoder")]
		////[Verify (MethodToProperty)]
		RTCVideoEncoder Vp8Encoder { get; }
	}

	// @interface RTCVideoEncoderVP9 : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderVP9
	{
		// +(id<RTCVideoEncoder>)vp9Encoder;
		[Static]
		[Export ("vp9Encoder")]
		////[Verify (MethodToProperty)]
		RTCVideoEncoder Vp9Encoder { get; }

		// +(_Bool)isSupported;
		[Static]
		[Export ("isSupported")]
		////[Verify (MethodToProperty)]
		bool IsSupported { get; }
	}

	// @interface RTCVideoEncoderAV1 : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCVideoEncoderAV1
	{
		// +(id<RTCVideoEncoder>)av1Encoder;
		[Static]
		[Export ("av1Encoder")]
		////[Verify (MethodToProperty)]
		RTCVideoEncoder Av1Encoder { get; }

		// +(_Bool)isSupported;
		[Static]
		[Export ("isSupported")]
		////[Verify (MethodToProperty)]
		bool IsSupported { get; }
    }

    // @interface RTCI420Buffer : NSObject <RTCI420Buffer>
    [BaseType (typeof(NSObject))]
    interface RTCI420Buffer : IRTCI420Buffer
    {
    }

    // @interface RTCMutableI420Buffer : RTCI420Buffer <RTCMutableI420Buffer>
    [BaseType (typeof(RTCI420Buffer))]
	interface RTCMutableI420Buffer : IRTCMutableI420Buffer
	{
	}

	// typedef void (^RTCCallbackLoggerMessageHandler)(NSString * _Nonnull);
	delegate void RTCCallbackLoggerMessageHandler (string arg0);

	// typedef void (^RTCCallbackLoggerMessageAndSeverityHandler)(NSString * _Nonnull, RTCLoggingSeverity);
	delegate void RTCCallbackLoggerMessageAndSeverityHandler (string arg0, RTCLoggingSeverity arg1);

	// @interface RTCCallbackLogger : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCCallbackLogger
	{
		// @property (assign, nonatomic) RTCLoggingSeverity severity;
		[Export ("severity", ArgumentSemantic.Assign)]
		RTCLoggingSeverity Severity { get; set; }

		// -(void)start:(RTCCallbackLoggerMessageHandler _Nullable)handler;
		[Export ("start:")]
		void Start ([NullAllowed] RTCCallbackLoggerMessageHandler handler);

		// -(void)startWithMessageAndSeverityHandler:(RTCCallbackLoggerMessageAndSeverityHandler _Nullable)handler;
		[Export ("startWithMessageAndSeverityHandler:")]
		void StartWithMessageAndSeverityHandler ([NullAllowed] RTCCallbackLoggerMessageAndSeverityHandler handler);

		// -(void)stop;
		[Export ("stop")]
		void Stop ();
	}

	// @interface RTCFileLogger : NSObject
	[BaseType (typeof(NSObject))]
	interface RTCFileLogger
	{
		// @property (assign, nonatomic) RTCFileLoggerSeverity severity;
		[Export ("severity", ArgumentSemantic.Assign)]
		RTCFileLoggerSeverity Severity { get; set; }

		// @property (readonly, nonatomic) RTCFileLoggerRotationType rotationType;
		[Export ("rotationType")]
		RTCFileLoggerRotationType RotationType { get; }

		// @property (assign, nonatomic) BOOL shouldDisableBuffering;
		[Export ("shouldDisableBuffering")]
		bool ShouldDisableBuffering { get; set; }

		// -(instancetype _Nonnull)initWithDirPath:(NSString * _Nonnull)dirPath maxFileSize:(NSUInteger)maxFileSize;
		[Export ("initWithDirPath:maxFileSize:")]
		NativeHandle Constructor (string dirPath, nuint maxFileSize);

		// -(instancetype _Nonnull)initWithDirPath:(NSString * _Nonnull)dirPath maxFileSize:(NSUInteger)maxFileSize rotationType:(RTCFileLoggerRotationType)rotationType __attribute__((objc_designated_initializer));
		[Export ("initWithDirPath:maxFileSize:rotationType:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string dirPath, nuint maxFileSize, RTCFileLoggerRotationType rotationType);

		// -(void)start;
		[Export ("start")]
		void Start ();

		// -(void)stop;
		[Export ("stop")]
		void Stop ();

		// -(NSData * _Nullable)logData;
		[NullAllowed, Export ("logData")]
		////[Verify (MethodToProperty)]
		NSData LogData { get; }
	}
}
