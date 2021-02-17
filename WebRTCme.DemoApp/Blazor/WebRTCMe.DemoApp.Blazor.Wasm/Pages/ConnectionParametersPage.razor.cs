using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Wasm.Pages
{
    public partial class ConnectionParametersPage
    {
        [Inject]
        ConnectionParametersViewModel ConnectionParametersViewModel {get;set;}

        [Parameter]
        public string TurnServerNamesJson { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (TurnServerNamesJson is not null)
            {
                var turnServerNames = JsonSerializer.Deserialize<string[]>(TurnServerNamesJson).ToList();
                ConnectionParametersViewModel.TurnServerNames = turnServerNames;
            }
        }
    }
}
