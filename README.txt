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


 


