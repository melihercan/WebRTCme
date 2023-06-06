WHICH BRANCH WITH RESPECT TO CHRMOMIUM
======================================
WebRTC follows the same release cycle as Chromium and the chromium dashboard provides a list of milestones 
(hence the M) and their associated branches: https://chromiumdash.appspot.com/branches 

To build follow instructions:
https://webrtc.googlesource.com/src/+/refs/heads/main/docs/native-code/index.md

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

WINDOWS
=======
https://webrtc.googlesource.com/src/+/refs/heads/main/docs/native-code/development/index.md
https://webrtc.googlesource.com/src/+/main/docs/native-code/development/prerequisite-sw/index.md
https://developpaper.com/compiling-webrtc-under-windows/

- install Chromium depot tools: https://commondatastorage.googleapis.com/chrome-infra-docs/flat/depot_tools/docs/html/depot_tools_tutorial.html#_setting_up
- create 'webrtc' folder, go there and 
    o Set env variables:
	  - GYP_MSVS_VERSION=2022
	  - DEPOT_TOOLS_WIN_TOOLCHAIN=0
	  - GYP_MSVS_OVERRIDE_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community
	  - vs2022_install=C:\Program Files\Microsoft Visual Studio\2022\Community
	  - GYP_GENERATORS=msvs-ninja,ninja
	o fetch --nohooks webrtc
	o gclient sync
	o cd src
	  ## view available branches
	o git branch -r 
	o git checkout branch-heads/4844 ##(m99) (or latest mxxx)
	o gclient sync
	  ## Compile vs2022 release
	o gn gen out\Release-vs2022 --ide=vs2022 --args="is_debug=false target_os=\"win\" target_cpu=\"x64\" is_component_build=false is_clang=false use_lld=false treat_warnings_as_errors=false use_rtti=true rtc_include_tests=false rtc_build_examples=true"
	o ninja -C out\Release-vs2022


TO CREATE DLL AND VS2022 PROJECT
================================
PS F:\dev\webrtc-checkout\src> (Get-Content BUILD.gn).replace('rtc_static_library', 'rtc_shared_library') | Set-Content BUILD.gn
PS F:\dev\webrtc-checkout\src> (Get-Content BUILD.gn) -notmatch 'complete_static_lib' | Set-Content BUILD.gn
PS F:\dev\webrtc-checkout\src> (Get-Content webrtc.gni).replace('!build_with_chromium && is_component_build', 'false') | Set-Content webrtc.gni
PS F:\dev\webrtc-checkout\src> (Get-Content rtc_tools\BUILD.gn) -notmatch ':frame_analyzer' | Set-Content rtc_tools\BUILD.gn
PS F:\dev\webrtc-checkout\src> gn gen out/vs2022 --ide=vs2022 --args="is_component_build=true rtc_enable_symbol_export=true rtc_include_tests=false rtc_build_tools=false rtc_build_examples=false"



MACOS
=====
https://groups.google.com/g/discuss-webrtc/c/MQbTcKvk-y4
https://github.com/tmthecoder/WebRTC-macOS
fetch --nohooks webrtc
gclient sync
gn gen out/mac_release --args='is_debug=false'
ninja -C out/mac_release rtc_sdk_objc


https://github.com/tmthecoder/WebRTC-macOS
Building
To Build your own WebRTC Framework, similar to the one I have build, the instructions are as follows:

Clone the WebRTC Source:

git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git
export PATH=$PATH:/path/to/depot_tools

fetch --nohooks webrtc_ios

gclient sync
Generate the macOS Targets (x86_64 and arm64)

gn gen out/mac_x64 --args='target_os="mac" target_cpu="x64" is_component_build=false is_debug=false rtc_libvpx_build_vp9=false enable_stripping=true rtc_enable_protobuf=false'

gn gen out/mac_arm64 --args='target_os="mac" target_cpu="arm64" is_component_build=false is_debug=false rtc_libvpx_build_vp9=false enable_stripping=true rtc_enable_protobuf=false'

Build both frameworks:

ninja -C out/mac_x64 mac_framework_objc

ninja -C out/mac_arm64 mac_framework_objc
Merge the frameworks using lipo:

cd ..

cp -R src/out/mac_x64/WebRTC.framework WebRTC.framework

lipo -create -output WebRTC.framework/WebRTC src/out/mac_x64/WebRTC.framework/WebRTC src/out/mac_arm64/WebRTC.framework/WebRTC
The outputted WebRTC.framework can be imported into an Xcode Project and will support both Intel and Apple Silicon Macs


USEFUL LINKS
============
https://webrtc.github.io/webrtc-org/native-code/native-apis/
https://w3c.github.io/webrtc-pc/
https://groups.google.com/g/discuss-webrtc/c/YZaNOPdWG2Y
https://groups.google.com/g/discuss-webrtc/c/SWIP9-PdmYw
https://github.com/webrtc-sdk/libwebrtc


BUILD ARGS
==========
gn args out\Default --list --short

