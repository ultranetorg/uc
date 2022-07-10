using UC.Net.Node.MAUI.Controls;
using UC.Net.Node.MAUI.Models;
using UC.Net.Node.MAUI.ViewModels;
using Rg.Plugins.Popup.Exceptions;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UC.Net.Node.MAUI.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountsPopup : PopupPage
    {
        private static AccountsPopup popup;
        public AccountsPopup()
        {
            InitializeComponent();
        }
       
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
        public async void Hide()
        {
            await Navigation.RemovePopupPageAsync(popup);
        }
        public static async Task Show()
        {
            popup = new AccountsPopup();
            await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
        }


    }

}