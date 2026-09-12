using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace WebRTCme.Android
{
    /// <summary>
    /// Keeps the app in the foreground for as long as it is capturing the screen.
    /// </summary>
    /// <remarks>
    /// Not optional and not a nicety. Since Android 10 a <c>MediaProjection</c> can only be
    /// obtained while a foreground service of type <c>mediaProjection</c> is already running, and
    /// "already" is the whole difficulty: <c>startForegroundService</c> returns immediately and the
    /// service reaches the foreground some time later, on the main looper. Taking the projection in
    /// between fails with a SecurityException naming the missing service - which reads as a
    /// manifest problem rather than as the race it is. <see cref="ReadyAsync"/> exists to close it.
    ///
    /// Android is unforgiving in the other direction too: a service started this way that does not
    /// call <c>startForeground</c> within a few seconds kills the process with
    /// <c>ForegroundServiceDidNotStartInTimeException</c>. So everything here is logged and nothing
    /// is allowed to throw quietly.
    /// </remarks>
    [Service(
        Exported = false,
        ForegroundServiceType = ForegroundService.TypeMediaProjection)]
    public class ScreenCaptureService : Service
    {
        const string ChannelId = "webrtcme.screencapture";
        const int NotificationId = 0x5C12;

        static TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Completes once the service is actually in the foreground and a projection may be taken.
        /// </summary>
        public static Task<bool> ReadyAsync => _ready.Task;

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Echo("ScreenCaptureService: OnStartCommand");

            try
            {
                var notification = BuildNotification();

                // The type has to be passed explicitly from Android 10. The two-argument overload
                // leaves the service typeless, and a typeless service is not one a projection will
                // accept - which surfaces later and somewhere else.
                // OperatingSystem rather than Build.VERSION.SdkInt, which reads the same but is
                // not a guard the platform-compatibility analyzer understands.
                if (OperatingSystem.IsAndroidVersionAtLeast(29))
                    StartForeground(NotificationId, notification, ForegroundService.TypeMediaProjection);
                else
                    StartForeground(NotificationId, notification);

                Echo("ScreenCaptureService: in the foreground");
                _ready.TrySetResult(true);
            }
            catch (Exception exception)
            {
                // Said out loud rather than swallowed: without this the only symptom is the process
                // being killed a few seconds later for not starting in time, which names nothing.
                Echo($"ScreenCaptureService: startForeground failed - " +
                    $"{exception.GetType().Name}: {exception.Message}");
                _ready.TrySetResult(false);
                StopSelf();
            }

            // Not sticky: if Android kills this, the projection goes with it, and restarting the
            // service would leave a notification for a capture that is not happening.
            return StartCommandResult.NotSticky;
        }

        public override void OnDestroy()
        {
            // Reset, so a later share waits for its own start rather than seeing the last one's.
            Interlocked.Exchange(ref _ready,
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously));

            base.OnDestroy();
        }

        Notification BuildNotification()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var manager = (NotificationManager)GetSystemService(NotificationService);

                // Low importance: a status indicator, not an interruption. Creating a channel that
                // already exists is a no-op, so there is nothing to check first.
                manager?.CreateNotificationChannel(new NotificationChannel(
                    ChannelId, "Screen sharing", NotificationImportance.Low));
            }

            return new Notification.Builder(this, ChannelId)
                .SetContentTitle("Sharing your screen")
                .SetContentText("This app is capturing the screen for a call.")
                .SetSmallIcon(global::Android.Resource.Drawable.PresenceVideoOnline)
                .SetOngoing(true)
                .Build();
        }

        public static void Start(Context context)
        {
            var intent = new Intent(context, typeof(ScreenCaptureService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else
                context.StartService(intent);
        }

        public static void Stop(Context context) =>
            context.StopService(new Intent(context, typeof(ScreenCaptureService)));

        static void Echo(string line)
        {
            Console.WriteLine($"######## {line}");
            System.Diagnostics.Debug.WriteLine($"######## {line}");
        }
    }
}
