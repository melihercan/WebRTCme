using System.ComponentModel;
using Xamarin.Forms;
using DemoApp.ViewModels;

namespace DemoApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}