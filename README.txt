I wanted to create a single cross platform library with Razor and possible Xamarin UI support.
But it seems we cannot compile Razor files with <Project Sdk="MSBuild.Sdk.Extras/2.0.54">.
So this library will be pure API (with JS interop).

Additionally I need to define RCL and Xamarin UI lib on top.