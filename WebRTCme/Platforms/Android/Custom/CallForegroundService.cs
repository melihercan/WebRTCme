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
    /// Keeps the app running, and keeps it holding the camera and microphone, for the length of a
    /// call.
    /// </summary>
    /// <remarks>
    /// Without this a call does not survive the user switching apps, and does not come back when
    /// they switch back. Measured on 2026-09-11: pressing Home closed the camera, froze media in
    /// both directions, and took both transports <c>connected -> disconnected -> failed</c>.
    /// Returning to the foreground recovered nothing - the camera was never reopened and the
    /// transports stayed failed - and an ICE restart did not help either. Signalling stayed alive
    /// throughout, so the app looked connected while carrying nothing.
    ///
    /// Two separate Android rules cause that, and this addresses both. A backgrounded app may not
    /// hold the camera or microphone, which needs the <c>camera</c> and <c>microphone</c> service
    /// types; and a cached process is frozen, which stops ICE answering - any foreground service
    /// prevents that.
    ///
    /// Started when local capture begins and stopped when the last local track stops, rather than
    /// by the app: the thing that knows a capture is live is the capture.
    /// </remarks>
    [Service(
        Exported = false,
        ForegroundServiceType = ForegroundService.TypeCamera | ForegroundService.TypeMicrophone)]
    public class CallForegroundService : Service
    {
        const string ChannelId = "webrtcme.call";
        const int NotificationId = 0xCA11;

        static TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Completes once the service is actually in the foreground.
        /// </summary>
        /// <remarks>
        /// Unlike the projection service this is not something a caller has to wait for - nothing
        /// here fails if capture starts a moment early. It exists so that a failure to start is
        /// observable rather than silent, which is how the projection one was found.
        /// </remarks>
        public static Task<bool> ReadyAsync => _ready.Task;

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Echo("CallForegroundService: OnStartCommand");

            try
            {
                var notification = BuildNotification();

                // The types have to be passed explicitly from Android 10; the two-argument overload
                // leaves the service typeless, and a typeless service does not keep the camera.
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    StartForeground(NotificationId, notification,
                        ForegroundService.TypeCamera | ForegroundService.TypeMicrophone);
                }
                else
                {
                    StartForeground(NotificationId, notification);
                }

                Echo("CallForegroundService: in the foreground");
                _ready.TrySetResult(true);
            }
            catch (Exception exception)
            {
                // Said out loud. A foreground service that does not start in time kills the
                // process a few seconds later with an exception naming nothing useful.
                Echo($"CallForegroundService: startForeground failed - " +
                    $"{exception.GetType().Name}: {exception.Message}");
                _ready.TrySetResult(false);
                StopSelf();
            }

            return StartCommandResult.NotSticky;
        }

        public override void OnDestroy()
        {
            Interlocked.Exchange(ref _ready,
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously));

            base.OnDestroy();
        }

        Notification BuildNotification()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var manager = (NotificationManager)GetSystemService(NotificationService);

                manager?.CreateNotificationChannel(new NotificationChannel(
                    ChannelId, "Calls", NotificationImportance.Low));
            }

            return new Notification.Builder(this, ChannelId)
                .SetContentTitle("In a call")
                .SetContentText("Camera and microphone are in use.")
                .SetSmallIcon(global::Android.Resource.Drawable.PresenceVideoOnline)
                .SetOngoing(true)
                .Build();
        }

        public static void Start(Context context)
        {
            var intent = new Intent(context, typeof(CallForegroundService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else
                context.StartService(intent);
        }

        public static void Stop(Context context) =>
            context.StopService(new Intent(context, typeof(CallForegroundService)));

        static void Echo(string line)
        {
            Console.WriteLine($"######## {line}");
            System.Diagnostics.Debug.WriteLine($"######## {line}");
        }
    }
}
