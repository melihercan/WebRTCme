namespace WebRTCme.DeviceTests.Runner;

public static class MauiProgram
{
    /// <summary>
    /// The smallest MAUI app that can exist. No logging builder: the scenarios report through
    /// Console.WriteLine and Debug.WriteLine directly, which is what reaches adb logcat and
    /// devicectl's console, and adding Microsoft.Extensions.Logging.Debug for AddDebug() would be a
    /// package to maintain for output nothing reads.
    /// </summary>
    public static MauiApp CreateMauiApp() =>
        MauiApp.CreateBuilder().UseMauiApp<App>().Build();
}
