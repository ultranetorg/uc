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
    public partial class PrivateKeyPage : CustomPage
    {
        PrivateKeyViewModel viewModel;
        public PrivateKeyPage(Wallet wallet)
        {
            InitializeComponent();
            BindingContext=viewModel=new PrivateKeyViewModel(this,wallet);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.OnAppearing();
        }
    }
    public class PrivateKeyViewModel : BaseViewModel
    {
       
        public PrivateKeyViewModel(Page page, Wallet wallet)
        {
            Page = page;
            Wallet = wallet;          
           

        }
        public Command DeleteCommand
        {
            get => new Command(Delete);
        }

        private async void Delete()
        {
           await DeleteAccountPopup.Show(Wallet);
        }
        internal void OnAppearing()
        {
           
        }
       
        CustomCollection<Author> _Authors = new CustomCollection<Author>();
        public CustomCollection<Author> Authors
        {
            get { return _Authors; }
            set { SetProperty(ref _Authors, value); }
        }
        CustomCollection<Product> _Products = new CustomCollection<Product>();
        public CustomCollection<Product> Products
        {
            get { return _Products; }
            set { SetProperty(ref _Products, value); }
        }


        public Page Page { get; }
        Wallet _Wallet;
        public Wallet Wallet
        {
            get { return _Wallet; }
            set { SetProperty(ref _Wallet, value); }
        }
       
    }
}
