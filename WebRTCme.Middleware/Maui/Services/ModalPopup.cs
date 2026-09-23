namespace WebRTCme.Middleware.Maui.Services
{
    /// <summary>
    /// Shows the middleware's alerts and prompts as the platform's own dialogs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Native dialogs, because anything else ends the call.</b> This used to show a
    /// CommunityToolkit.Maui popup, and since that library's v12 rewrite a popup is a modal page:
    /// <c>ShowPopupAsync</c> pushes a <c>PopupPage</c> with <c>PushModalAsync</c>. Pushing a page is
    /// navigation, so the call page underneath receives <c>OnDisappearing</c> - which is exactly
    /// where a call page is told to leave the call and release the camera. Every error shown during
    /// a call therefore hung up on it, and dismissing the error rejoined from scratch.
    /// </para>
    /// <para>
    /// Found on Android on 2026-09-23: pressing Restart ICE on the answering side is refused, as
    /// it should be, and the refusal's dialog sent <c>LeaveAsync</c> 300 ms later. The call stayed
    /// down, camera closed, for as long as the dialog was on screen. The same held for all nine
    /// call sites, on every MAUI platform - a failed mute, a failed screen share, and the
    /// connection-error path, which then tried to reconnect with streams the page had just
    /// released.
    /// </para>
    /// <para>
    /// An alert or prompt is not navigation on any platform - an <c>AlertDialog</c>, a
    /// <c>UIAlertController</c>, a <c>ContentDialog</c> - so the page underneath never disappears.
    /// It also takes its colours from the platform, which the toolkit popup never did: it painted
    /// itself white in a dark app and rendered its error text white-on-white.
    /// </para>
    /// <para>
    /// <see cref="GenericPopupIn.Image"/> is not shown. Nothing sets it, and the resource path the
    /// old popup built for it pointed into another application's assembly.
    /// </para>
    /// </remarks>
    public class ModalPopup : IModalPopup
    {
        public Task<GenericPopupOut> GenericPopupAsync(GenericPopupIn genericPopupIn) =>
            // Several callers are on a thread-pool thread - the connection's error handler is an
            // Rx callback - and a dialog can only be raised from the UI thread.
            MainThread.InvokeOnMainThreadAsync(() => ShowAsync(genericPopupIn));

        static async Task<GenericPopupOut> ShowAsync(GenericPopupIn popupIn)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page
                ?? throw new InvalidOperationException("There is no page to show the dialog on.");

            var title = popupIn.Title ?? string.Empty;
            var text = popupIn.Text ?? string.Empty;
            var ok = popupIn.Ok ?? "OK";

            if (popupIn.EntryPlaceholder is not null)
            {
                // Null when cancelled, which is how the old popup's Ok = false read as well.
                var entry = await page.DisplayPromptAsync(
                    title, text, ok, popupIn.Cancel ?? "Cancel", popupIn.EntryPlaceholder);
                return new GenericPopupOut { Ok = entry is not null, Entry = entry };
            }

            if (popupIn.Cancel is not null)
            {
                var accepted = await page.DisplayAlertAsync(title, text, ok, popupIn.Cancel);
                return new GenericPopupOut { Ok = accepted };
            }

            await page.DisplayAlertAsync(title, text, ok);
            return new GenericPopupOut { Ok = true };
        }
    }
}
