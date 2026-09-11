using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media.Projection;
using Android.OS;

namespace WebRTCme.Android
{
    /// <summary>
    /// Asks the user to allow screen capture, and hands the answer back to whoever asked.
    /// </summary>
    /// <remarks>
    /// An activity of its own, rather than a call into the host app's activity, so that a consumer
    /// of this library needs to write nothing: screen capture permission can only be requested
    /// with <c>startActivityForResult</c>, and the result only arrives at the activity that asked.
    /// Requiring every app to override <c>OnActivityResult</c> and forward it here would make this
    /// feature something a consumer has to wire up rather than something they can call.
    ///
    /// Transparent and immediately finished, so the user sees only the system dialog. The
    /// permission itself is a one-shot grant - Android hands back an Intent that authorises a
    /// single capture session, and asking again means another dialog.
    /// </remarks>
    [Activity(
        Theme = "@android:style/Theme.Translucent.NoTitleBar",
        ExcludeFromRecents = true,
        NoHistory = true)]
    public class ScreenCapturePermissionActivity : Activity
    {
        const int RequestCode = 0x5C12;

        // One request at a time. Two overlapping screen shares is not a case worth supporting, and
        // a second dialog while the first is open is confusing rather than useful.
        static TaskCompletionSource<Intent> _pending;

        /// <summary>
        /// Shows the system dialog and completes with the granted intent, or null if refused.
        /// </summary>
        public static Task<Intent> RequestAsync(Context context)
        {
            var pending = new TaskCompletionSource<Intent>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var existing = Interlocked.CompareExchange(ref _pending, pending, null);
            if (existing is not null)
                return existing.Task;

            var intent = new Intent(context, typeof(ScreenCapturePermissionActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);

            return pending.Task;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var manager = (MediaProjectionManager)GetSystemService(MediaProjectionService);
            if (manager is null)
            {
                Complete(null);
                return;
            }

            StartActivityForResult(manager.CreateScreenCaptureIntent(), RequestCode);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode != RequestCode)
                return;

            // Refusal arrives as a non-OK result, and so does dismissing the dialog. Null rather
            // than an exception: the caller turns it into one with a message that says what the
            // user actually did.
            Complete(resultCode == Result.Ok ? data : null);
        }

        void Complete(Intent data)
        {
            Interlocked.Exchange(ref _pending, null)?.TrySetResult(data);
            Finish();
        }
    }
}
