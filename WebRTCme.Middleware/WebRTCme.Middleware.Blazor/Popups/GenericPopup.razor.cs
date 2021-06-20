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

        Dialog _dialog;
        GenericPopupIn _in;
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

        public async Task OnBeforeShow(DialogBeforeShowEventArgs e)
        {
            _dialog = e.Dialog;
            _in = (GenericPopupIn)_dialog.Input;
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


    }
}
