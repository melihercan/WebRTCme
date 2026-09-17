using Microsoft.AspNetCore.Components;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Pages
{
    partial class CallPage : IAsyncDisposable
    {
        [Inject]
        CallViewModel CallViewModel { get; set; }

        [Inject]
        NavigationManager Navigation { get; set; }

        [Parameter]
        public string ConnectionParametersJson { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var connectionParameters = JsonSerializer.Deserialize<ConnectionParameters>(ConnectionParametersJson);
            await CallViewModel.OnPageAppearingAsync(connectionParameters, ReRender);
        }

        void ReRender()
        {
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Leaves the call.
        /// </summary>
        /// <remarks>
        /// Navigating away is the whole of it: the router disposes this page on the way out and
        /// <see cref="DisposeAsync"/> is what tears the call down. Doing it in that order rather
        /// than tearing down first means there is exactly one teardown path, the one that already
        /// worked when people left with the browser's back button.
        /// </remarks>
        void LeaveCall() => Navigation.NavigateTo("/");

        /// <summary>
        /// Hangs up, and waits for it.
        /// </summary>
        /// <remarks>
        /// IAsyncDisposable rather than IDisposable, which answers the TODO its sibling carried for
        /// four years: Blazor awaits DisposeAsync on a component that implements it, so the
        /// teardown no longer has to be started and abandoned. It matters more here than on the
        /// chat page - hanging up releases the camera and the microphone, and fire-and-forget left
        /// them running for however long the task took to get there.
        ///
        /// Suggested in issue #17.
        /// </remarks>
        public async ValueTask DisposeAsync() => await CallViewModel.OnPageDisappearingAsync();
    }
}
