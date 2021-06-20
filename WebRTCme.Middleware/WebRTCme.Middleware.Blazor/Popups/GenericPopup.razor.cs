using BlazorDialog;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
//namespace WebRTCme.DemoApp.Blazor.Pages
{
    public partial class GenericPopup
    {
        GenericPopupOut _out = new();

        async Task OnOkAsync(Dialog dialog)
        {
            _out.Ok = true;    
            await dialog.Hide(_out);
        }

        async Task OnCancelAsync(Dialog dialog)
        {
            _out.Ok = false;
            await dialog.Hide(_out);
        }

#if false
        public async Task OnBeforeShow(DialogBeforeShowEventArgs e)
        {
        }

        public async Task OnAfterShow(DialogAfterShowEventArgs e)
        {
        }

        public async Task OnBeforeHide(DialogBeforeHideEventArgs e)
        {
        }

        public async Task OnAfterHide(DialogAfterHideEventArgs e)
        {
        }
#endif

    }
}
