using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware.Xamarin.Popups;
using Xamarin.CommunityToolkit.Extensions;

namespace WebRTCme.Middleware.Xamarin.Services
{
    public class ModalPopup : IModalPopup
    {
        public Task<GenericPopupOut> GenericPopupAsync(GenericPopupIn genericPopupIn)
        {
            return new GenericPopup().PopupAsync(genericPopupIn);
        }
    }
}
