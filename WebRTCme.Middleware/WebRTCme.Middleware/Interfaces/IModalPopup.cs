using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IModalPopup
    {
        Task<GenericPopupOut> GenericPopupAsync(GenericPopupIn genericPopupIn);
    }
}
