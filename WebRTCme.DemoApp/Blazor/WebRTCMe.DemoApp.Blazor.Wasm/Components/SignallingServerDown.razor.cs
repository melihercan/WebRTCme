using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.DemoApp.Blazor.Wasm.Components
{
    public partial class SignallingServerDown
    {
        [CascadingParameter]
        BlazoredModalInstance Modal { get; set; }

        private async void OnOk()
        {
            await Modal.CloseAsync();
        }
    }
}
