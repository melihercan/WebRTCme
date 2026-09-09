using WebRTCme.Middleware.Maui.Popups;

namespace WebRTCme.Middleware.Maui.Services
{
    public class ModalPopup : IModalPopup
    {
        public Task<GenericPopupOut> GenericPopupAsync(GenericPopupIn genericPopupIn)
        {
            return new GenericPopup().PopupAsync(genericPopupIn);
        }
    }
}