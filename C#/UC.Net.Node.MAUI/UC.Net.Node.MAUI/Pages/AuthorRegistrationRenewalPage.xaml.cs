using UC.Net.Node.MAUI.Controls;
using UC.Net.Node.MAUI.Helpers;
using UC.Net.Node.MAUI.Models;
using UC.Net.Node.MAUI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace UC.Net.Node.MAUI.Pages
{
    public partial class AuthorRegistrationRenewalPage : CustomPage
    {
        AuthorRegistrationRenewalViewModel viewModel;
        public AuthorRegistrationRenewalPage()
        {
            InitializeComponent();
            BindingContext=viewModel=new AuthorRegistrationRenewalViewModel(this);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.OnAppearing();
        }
    }
    public class AuthorRegistrationRenewalViewModel : BaseViewModel
    {
       
        public AuthorRegistrationRenewalViewModel(Page page)
        {
            Page = page;
        

        }
        public Command ColorTappedCommand
        {
            get => new Command<AccountColor>(ColorTapped);
        }

        private void ColorTapped(AccountColor obj)
        {

            foreach (var item in Colors)
            {
                item.BoderColor = Color.Transparent;
                Colors.ReportItemChange(item);
            }
            obj.BoderColor =Page.BackgroundColor;
            Colors.ReportItemChange(obj);
            Wallet.AccountColor = obj.Color;
        }
        public Command CloseCommad
        {
            get => new Command(Close);
        }

        private async void Close()
        {
            await Page.Navigation.PopModalAsync();
        }
        public Command PrevCommand
        {
            get => new Command(Prev);
        }
        private void Prev()
        {
            Position -= 1;
        }
        public Command NextCommad
        {
            get => new Command(Next);
        }
        private void Next()
        {
            if (Position == 1) return;
            Position += 1;
        }
        internal void OnAppearing()
        {
           
        }
        CustomCollection<AccountColor> _Colors = new CustomCollection<AccountColor>();
        public CustomCollection<AccountColor> Colors
        {
            get { return _Colors; }
            set { SetProperty(ref _Colors, value); }
        }
      
        public Page Page { get; }
       
        int _Position;
        public int Position
        {
            get { return _Position; }
            set { SetProperty(ref _Position, value); }
        }
        Wallet _Wallet=new Wallet();
        public Wallet Wallet
        {
            get { return _Wallet; }
            set { SetProperty(ref _Wallet, value); }
        }
        
    }
}
