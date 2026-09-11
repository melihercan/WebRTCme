using Android.App;

// Declared by the library rather than by each consuming app, so that screen capture is something
// an app can simply call. These reach a consumer's manifest through the manifest merger, alongside
// the [Activity] and [Service] attributes on the two types beside this file.
//
// FOREGROUND_SERVICE_MEDIA_PROJECTION is the one that is easy to miss: it became mandatory for
// apps targeting Android 14, and without it the service starts and is then killed the moment it
// declares its type - which reads as the capture failing rather than as a missing permission.
[assembly: UsesPermission(global::Android.Manifest.Permission.ForegroundService)]
[assembly: UsesPermission(global::Android.Manifest.Permission.ForegroundServiceMediaProjection)]
[assembly: UsesPermission(global::Android.Manifest.Permission.PostNotifications)]
