using ObjCRuntime;
using UIKit;

namespace WebRTCme.DeviceTests.Runner;

public class Program
{
    // The filter comes from the command line here rather than an intent: devicectl passes arguments
    // through to the app, so the harness can run one scenario in a fresh process the same way it
    // does on Android.
    static void Main(string[] args)
    {
        App.Filter = args.FirstOrDefault(a => a.StartsWith("--filter="))?["--filter=".Length..];
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
