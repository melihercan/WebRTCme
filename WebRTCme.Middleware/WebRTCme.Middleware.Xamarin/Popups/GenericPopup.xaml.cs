using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.Converters;
using System.Reflection;


namespace WebRTCme.Middleware.Xamarin.Popups
{
//    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GenericPopup : Popup<GenericPopupOut>
    {
        private GenericPopupIn _in;
        private GenericPopupOut _out = new();

        public GenericPopup()
        {
            InitializeComponent();
        }

        public Task<GenericPopupOut> PopupAsync(GenericPopupIn genericPopupIn)
        {
            _in = genericPopupIn;
            if (_in.Image is not null)
            {
                Image.Source = ImageSource.FromResource($"DirectCall.UserInterface.Xamarin.Images.{_in.Image}", 
                    typeof(GenericPopup).GetTypeInfo().Assembly);
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

            return Shell.Current.Navigation.ShowPopupAsync(this);
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            _out.Ok = true;
            _out.Entry = _in.EntryPlaceholder is null ? null : Entry.Text;
            Dismiss(_out);
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            _out.Ok = false;
            _out.Entry = null;
            Dismiss(_out);
        }

        protected override GenericPopupOut GetLightDismissResult()
        {
            _out.Ok = false;
            _out.Entry = null;
            return _out;
        }
    }
}