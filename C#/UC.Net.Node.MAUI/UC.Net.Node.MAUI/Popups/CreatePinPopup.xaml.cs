﻿using UC.Net.Node.MAUI.Models;
using Rg.Plugins.Popup.Exceptions;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UC.Net.Node.MAUI.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreatePinPopup : PopupPage
    {
        private static CreatePinPopup popup;

        public CreatePinPopup( )
        {
            InitializeComponent();
            BindingContext = this;
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
            popup = new CreatePinPopup();
            await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
        }


    }
    
}