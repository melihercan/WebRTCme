using Android.App;
using Android.Content.PM;
using Android.OS;

namespace WebRTCme.DeviceTests.Runner;

/// <summary>
/// The Android entry point.
/// </summary>
/// <remarks>
/// <para>
/// <c>Exported</c> so the harness can start it with <c>adb shell am start</c> rather than tapping an
/// icon, and <c>LaunchMode.SingleTop</c> so a second start with a different filter reuses the
/// process instead of stacking activities.
/// </para>
/// <para>
/// The filter arrives as an intent extra:
/// </para>
/// <code>
/// adb shell am start -n com.melihercan.webrtcme.devicetests/.MainActivity --es filter ACompletedCall
/// </code>
/// <para>
/// which is how the harness runs one scenario in a process where no call has happened - the
/// device-enumeration check is meaningless otherwise.
/// </para>
/// </remarks>
[Activity(
    // Named explicitly. Without this MAUI generates one from a hash of the namespace -
    // crc647233123eb0837c50.MainActivity - which the harness would have to hardcode, and which
    // changes silently if the namespace is ever renamed. A stated name is one the harness can rely
    // on and a person can read.
    Name = "com.melihercan.webrtcme.devicetests.MainActivity",
    Label = "WebRTCme Device Tests",
    // Without an AppCompat descendant here the activity dies inflating its own layout, before
    // a single scenario runs. See Resources/values/styles.xml.
    Theme = "@style/WebRTCmeDeviceTests",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode
                           | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        App.Filter = Intent?.GetStringExtra("filter");
        base.OnCreate(savedInstanceState);
    }
}
