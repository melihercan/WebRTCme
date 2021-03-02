CREATION
========
- (from home folder) mkdir webrtc
- cd webrtc
- git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git
- export PATH:$PATH:/(path to depot_tools)
- (for Android) fetch --nohooks webrtc_android
- (for iOS) fetch –nohooks webrtc_ios
- gclient sync
- cd src
- (for Android) ./tools_webrtc/android/build_aar.py
- (for iOS) ./tools_webrtc/ios/build_ios_libs.py

iOS
===
- (folder 'out_ios_libs' contains 'WebRTC.framework') 
- cd ..
- cp -r src/WebRTC.framework .
- (install sharpie from 'https://aka.ms/objective-sharpie')
- sharpie update
- (open all headers and replace <WebRTC/xxx.h> --> "xxx.h")
- sharpie bind -sdk iphoneos -output ./ -namespace Webrtc -scope ./WebRTC.framework/Headers ./WebRTC.framework/Headers/WebRTC.h
- (sharpie should now have created two new files in the directory:
	ApiDefinitions.cs
	StructsAndEnums.cs
- (import all files to binding project)
