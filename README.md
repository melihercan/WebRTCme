# 10 Feb 24 - V2.0.0 HAS BEEN RELEASED
This release brings major upgrades with breaking changes, including platform and tool changes. For further details and the project's history, please refer to the previous [README](https://github.com/melihercan/WebRTCme/blob/master/README_V1.md). 

Added: 
- .NET MAUI 
- .NET 8 

Dropped: 
- Xamarin 
- .NET 5, 6, and 7 

The Xamarin code has been left in the repo but excluded from the solution file. 

Special thanks and credits to [Gøran Yri](https://github.com/EagleDelux) for his major contributions to .NET MAUI porting. 

Currently, MediaSoup is only working with Blazor. There are issues on both Android and iOS that I hope to fix soon.


![alt text](https://github.com/melihercan/WebRTCme/blob/master/non-code/docs/LibrariesAndPackages.png)

There are three NuGet packages available for different use cases: 

WebRTCme.Bindings: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.Bindings.svg)](https://www.nuget.org/packages/WebRTCme.Bindings)

WebRTCme: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.svg)](https://www.nuget.org/packages/WebRTCme)

WebRTCme.Middleware: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.Middleware.svg)](https://www.nuget.org/packages/WebRTCme.Middleware)

## Use case 1 - Native WebRTC Bindings
- Use the Bindings NuGet package
- Does not include a cross-platform API
- Only provides native platform-specific APIs
- For desktop, use [SIPSorcery](https://sipsorcery-org.github.io/sipsorcery/) directly

## Use case 2 - Cross-platform library 
- Use the WebRTCme NuGet package
- This package includes the bindings and provides a cross-platform API
- You can create your own middleware or app on top of this library 

## Use case 3 - Middleware
- Use the Middleware NuGet package
- This package contains the bindings, core library, media elements and signaling server clients
- You can directly create your app on top of this middleware


