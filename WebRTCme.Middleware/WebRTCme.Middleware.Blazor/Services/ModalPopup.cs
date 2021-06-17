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
        [Inject]
        IBlazorDialogService BlazorDialog { get; set; }

        public Task<GenericPopupOut> GenericPopupAsync(GenericPopupIn genericPopupIn)
        {
            return BlazorDialog.ShowDialog<GenericPopupOut>("GenericPopup", genericPopupIn);
        }
    }
}
