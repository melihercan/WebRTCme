using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Pages
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

            var turnServerNames = TurnServerNamesJson is null ? null : 
                JsonSerializer.Deserialize<string[]>(TurnServerNamesJson);
            ConnectionParametersViewModel.OnPageAppearing(turnServerNames);
        }
    }
}
