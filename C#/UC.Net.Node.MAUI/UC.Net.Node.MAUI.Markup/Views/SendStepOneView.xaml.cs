using UC.Net.Node.MAUI.Controls;
using UC.Net.Node.MAUI.Models;
using UC.Net.Node.MAUI.Popups;
using UC.Net.Node.MAUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UC.Net.Node.MAUI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendStepOneView : ContentView
    {
        SendStepOneViewModel viewModel;
        public SendStepOneView()
        {
            InitializeComponent();
            BindingContext= viewModel=new SendStepOneViewModel();   
        }
    }
    public class SendStepOneViewModel : BaseViewModel
    {
        public SendStepOneViewModel()
        {
           

        }
        
        public Command SourceTppedCommand
        {
            get => new Command(SourceTpped);
        }
        private async void SourceTpped()
        {
            await SourceAccountPopup.Show();
        }
        public Command RecipientTppedCommand
        {
            get => new Command(RecipientTpped);
        }
        private async void RecipientTpped()
        {
            await RecipientAccountPopup.Show();
        }

        Wallet _SourceWallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47FO",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromHex("#56d7de"),

        };
        public Wallet SourceWallet
        {
            get { return _SourceWallet; }
            set { SetProperty(ref _SourceWallet, value); }
        }
        Wallet _RecipientWallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47F0",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromHex("#6601e3"),
        };
        public Wallet RecipientWallet
        {
            get { return _RecipientWallet; }
            set { SetProperty(ref _RecipientWallet, value); }
        }
    }

}