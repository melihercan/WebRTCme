using Foundation;

namespace WebRTCme.DeviceTests.Runner;

/// <summary>
/// The scene Info.plist declares - MAUI's own, unchanged. Its existence is the point: from the iOS 27 and Mac Catalyst 27
/// SDK an app that does not adopt the UIScene life cycle is not launched.
/// </summary>
[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
}
