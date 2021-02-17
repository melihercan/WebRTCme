using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.DemoApp.Blazor.Wasm.Pages
{
    public partial class ConnectionParametersPage
    {
        [Parameter]
        public string TurnServerNamesJson { get; set; }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }
    }
}
