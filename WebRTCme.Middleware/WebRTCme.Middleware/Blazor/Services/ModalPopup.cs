using BlazorDialog;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware.Blazor.Services
{
    public class ModalPopup : IModalPopup
    {
        readonly IBlazorDialogService _blazorDialog;

        public ModalPopup(IBlazorDialogService blazorDialog)
        {
            _blazorDialog = blazorDialog;
        }

        public async Task<GenericPopupOut> GenericPopupAsync(GenericPopupIn genericPopupIn) => 
            await _blazorDialog.ShowDialog<GenericPopupOut>("GenericPopup", genericPopupIn);
    }
}
