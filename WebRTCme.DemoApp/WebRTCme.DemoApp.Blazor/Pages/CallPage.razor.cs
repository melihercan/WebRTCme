using Microsoft.AspNetCore.Components;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Pages
{
    partial class CallPage : IDisposable
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
        /// <see cref="Dispose"/> is what tears the call down. Doing it in that order rather than
        /// tearing down first means there is exactly one teardown path, the one that already
        /// worked when people left with the browser's back button.
        /// </remarks>
        void LeaveCall() => Navigation.NavigateTo("/");

        public void Dispose()
        {
            Task.Run(async () => await CallViewModel.OnPageDisappearingAsync());
        }
    }
}
