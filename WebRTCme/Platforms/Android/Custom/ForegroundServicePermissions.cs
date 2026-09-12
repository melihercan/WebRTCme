using Android.App;

// Declared by the library rather than by each consuming app, so that screen capture and calls are
// things an app can simply do. These reach a consumer's manifest through the manifest merger,
// alongside the [Activity] and [Service] attributes on the types beside this file.
//
// The typed ones became mandatory for apps targeting Android 14, and their absence is easy to
// misread: the service starts, declares its type, and is then killed - which reads as the capture
// failing rather than as a missing permission.
[assembly: UsesPermission(global::Android.Manifest.Permission.ForegroundService)]
[assembly: UsesPermission(global::Android.Manifest.Permission.ForegroundServiceMediaProjection)]
[assembly: UsesPermission(global::Android.Manifest.Permission.ForegroundServiceCamera)]
[assembly: UsesPermission(global::Android.Manifest.Permission.ForegroundServiceMicrophone)]
[assembly: UsesPermission(global::Android.Manifest.Permission.PostNotifications)]
