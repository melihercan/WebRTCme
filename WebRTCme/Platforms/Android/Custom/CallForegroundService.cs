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

        // Where the service is, so a stop that comes while it is still starting waits for it. A
        // service started with startForegroundService has to call startForeground, and one stopped
        // before Android has even run OnStartCommand never does: since Android 12 that kills the
        // app with ForegroundServiceDidNotStartInTimeException. A capture that starts and stops
        // within a moment does exactly that - it killed the device tests' ASenderWithNoTrackTakesOne
        // on CI on 2026-10-24, with no line from OnStartCommand before it.
        enum State { Idle, Starting, Running }
        static readonly object _gate = new();
        static State _state = State.Idle;
        static bool _stopWhenStarted;

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

                // The types have to be passed explicitly, because the two-argument overload leaves
                // the service typeless and a typeless service does not keep the camera. These two
                // types arrived in Android 11 - one version later than the projection type next
                // door - and below that the untyped call is all there is.
                //
                // OperatingSystem rather than Build.VERSION.SdkInt, which reads the same but is
                // not a guard the platform-compatibility analyzer understands.
                if (OperatingSystem.IsAndroidVersionAtLeast(30))
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

                // Asked to stop while it was starting: now that the promise to Android is kept, it can.
                bool stop;
                lock (_gate)
                {
                    _state = State.Running;
                    stop = _stopWhenStarted;
                    _stopWhenStarted = false;
                }
                if (stop)
                {
                    Echo("CallForegroundService: stopping, as asked while it started");
                    StopSelf();
                }
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
            lock (_gate)
            {
                _state = State.Idle;
                _stopWhenStarted = false;
            }
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
            lock (_gate)
            {
                // Already starting or running: a stop asked for meanwhile is called off.
                _stopWhenStarted = false;
                if (_state != State.Idle)
                    return;
                _state = State.Starting;
            }

            var intent = new Intent(context, typeof(CallForegroundService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else
                context.StartService(intent);
        }

        /// <summary>
        /// Stops the service - at once if it is running, or as soon as it is in the foreground if it
        /// is still starting, since stopping it before then kills the app (see the state above).
        /// </summary>
        public static void Stop(Context context)
        {
            lock (_gate)
            {
                if (_state == State.Starting)
                {
                    _stopWhenStarted = true;
                    return;
                }
                if (_state == State.Idle)
                    return;
            }
            context.StopService(new Intent(context, typeof(CallForegroundService)));
        }

        static void Echo(string line)
        {
            Console.WriteLine($"######## {line}");
            System.Diagnostics.Debug.WriteLine($"######## {line}");
        }
    }
}
