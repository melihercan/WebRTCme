I wanted to create a single cross platform library with Razor and possible Xamarin UI support.
But it seems we cannot compile Razor files with <Project Sdk="MSBuild.Sdk.Extras/2.0.54">.
So this library will be pure API (with JS interop).

Additionally I need to define RCL and Xamarin UI lib on top.

First of all, credit to Mr. Remi Bourgarel. He provided an excellent article about how to handle JS object references on Blazor .NET side:
https://remibou.github.io/How-to-keep-js-object-reference-in-Blazor/
Additionally he provided a github library "BrowserInterop" which makes developers life easier to use JSInterop:
https://github.com/RemiBou/BrowserInterop
I am inspired and also modified some code from this project.
Please see the license file: "LICENSE.BrowserInterop".




iOS bindings (install sharpie first and cocoapods:"sudo gem install cocoapods")
macOS open terminal:
- cd Projects
- pod lib create WebRTC
- mkdir WebRTC
- cd WebRTC
- pod setup
- pod init
- Edit Podfile, clean all and add this:
source 'https://github.com/CocoaPods/Specs.git'
target 'GoogleWebRTC' do
 platform :ios, '14.0'
 pod 'GoogleWebRTC'
end
- pod install
- edit WebRTC.h file and remove all WebRTC/ from imports
##- sharpie bind -o WebRTC.iOS -sdk iphoneos14.0 -namespace="WebRTC" -scope Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers  Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers/WebRTC.h -c -I Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers -arch arm64

##sharpie bind -o WebRTC.iOS -sdk iphoneos14.0 -scope Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers  Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers/WebRTC.h -c -I Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers -arch arm64

##sharpie bind -o WebRTC.iOS -sdk iphoneos14.0 -scope Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers  Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers/WebRTC.h -c -I Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers -arch arm64

 sharpie bind -o WebRTC.iOS -sdk iphoneos14.0 -scope Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers  Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers/WebRTC.h



1.) Created an ObjC static lib "WebRTCContiner", removed all sources.
2.)
- pod init
- Edit Podfile, clean all and add this:
source 'https://github.com/CocoaPods/Specs.git'
target 'WebRTCContainer' do
 platform :ios, '14.0'
 pod 'GoogleWebRTC'
end
- pod install
3.) edit WebRTC.h file and convert all <WebRTC/xxx.h> into "xxx.h"
sharpie bind -o WebRTC.iOS -sdk iphoneos14.0 -scope Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers  Pods/GoogleWebRTC/Frameworks/frameworks/WebRTC.framework/Headers/WebRTC.h
<
