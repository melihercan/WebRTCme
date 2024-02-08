using System.Reflection;
using CommunityToolkit.Maui.Views;

namespace WebRTCme.Middleware.Maui.Popups
{
    public partial class GenericPopup
    {
        private GenericPopupIn _in;
        private GenericPopupOut _out = new();

        public GenericPopup()
        {
            InitializeComponent();
        }

        public async Task<GenericPopupOut> PopupAsync(GenericPopupIn genericPopupIn)
        {
            _in = genericPopupIn;
            if (_in.Image is not null)
            {
                Image.Source = ImageSource.FromResource(
                    $"DirectCall.UserInterface.Maui.Images.{_in.Image}",
                    typeof(GenericPopup).GetTypeInfo().Assembly
                );
                Image.IsVisible = true;
            }

            if (_in.Title is not null)
            {
                Title.Text = _in.Title;
                Title.IsVisible = true;
            }

            if (_in.Text is not null)
            {
                Text.Text = _in.Text;
                Text.IsVisible = true;
            }

            if (_in.EntryPlaceholder is not null)
            {
                Entry.Placeholder = _in.EntryPlaceholder;
                Entry.IsVisible = true;
            }

            if (_in.Ok is not null)
            {
                OkButton.Text = _in.Ok;
                OkButton.IsVisible = true;
            }

            if (_in.Cancel is not null)
            {
                CancelButton.Text = _in.Cancel;
                CancelButton.IsVisible = true;
            }

            return await Shell.Current.ShowPopupAsync(this) as GenericPopupOut;
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            _out.Ok = true;
            _out.Entry = _in.EntryPlaceholder is null ? null : Entry.Text;
            Close(_out);
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            _out.Ok = false;
            _out.Entry = null;
            Close(_out);
        }
    }
}