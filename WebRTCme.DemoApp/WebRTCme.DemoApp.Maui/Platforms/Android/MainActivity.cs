using Android.App;
using Android.Content.PM;
using Android.OS;

namespace WebRTCme.DemoApp.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    /// <summary>
    /// Portrait, on every Android device.
    /// </summary>
    /// <remarks>
    /// <para>One rule rather than a phone/tablet split, because the camera is the reason and the
    /// camera does not care how big the screen is: the capture pipeline produces frames that are
    /// upright in portrait and lying on their side in landscape. Rotating the device does not
    /// rotate the picture, so allowing landscape means shipping a sideways video feed.</para>
    /// <para>UserPortrait rather than Portrait: upside-down is still portrait, and refusing it
    /// only annoys somebody holding the phone the other way up.</para>
    /// <para>Worth revisiting once the frame rotation follows the display - at that point a tablet
    /// in landscape is a genuinely better layout, since it has width for a row of tiles. Until
    /// then this is the setting that always looks right.</para>
    /// </remarks>
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        RequestedOrientation = ScreenOrientation.UserPortrait;
    }
}
