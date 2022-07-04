using UC.Net.Node.MAUI.Controls;
using UC.Net.Node.MAUI.Helpers;
using UC.Net.Node.MAUI.Models;
using UC.Net.Node.MAUI.Popups;
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
    public partial class AboutPage : CustomPage
    {
        AboutViewModel viewModel;
        public AboutPage()
        {
            InitializeComponent();
            BindingContext=viewModel=new AboutViewModel(this);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.OnAppearing();
        }
    }
    public class AboutViewModel : BaseViewModel
    {
       
        public AboutViewModel(Page page)
        {
            Page = page;
          
        }
        public Command CancelCommand
        {
            get => new Command(Cancel);
        }

        private async void Cancel()
        {
           await Page.Navigation.PopAsync();
        }
        internal void OnAppearing()
        {
           
        }

        public Command TransactionsCommand
        {
            get => new Command(Transactions);
        }

        private async void Transactions()
        {
            await Page.Navigation.PushAsync(new TransactionsPage());
        }
       
        public Page Page { get; }
        
    }
}
