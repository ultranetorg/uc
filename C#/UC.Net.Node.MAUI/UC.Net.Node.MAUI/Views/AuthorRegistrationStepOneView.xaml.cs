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
    public partial class AuthorRegistrationStepOneView : ContentView
    {
        AuthorRegistrationStepOneViewModel _viewModel;
        public AuthorRegistrationStepOneView()
        {
            InitializeComponent();
            BindingContext= _viewModel= new AuthorRegistrationStepOneViewModel();
        }
    }
    public class AuthorRegistrationStepOneViewModel : BaseViewModel
    {
        public AuthorRegistrationStepOneViewModel()
        {
            

        }
      
        public Command SelectAuthorCommand
        {
            get => new Command(SelectAuthor);
        }
        private async void SelectAuthor()
        {
            var author = await SelectAuthorPopup.Show();
            if(author == null)
                return;
            Author = author;
        }
        public Command SelectAccountCommand
        {
            get => new Command(SelectAccount);
        }
        private async void SelectAccount()
        {
            var wallet = await SourceAccountPopup.Show();
            if(wallet == null)
                return ;
            Wallet = wallet;
        }

        Wallet _Wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47F0",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromHex("#6601e3"),
        };
        public Wallet Wallet
        {
            get { return _Wallet; }
            set { SetProperty(ref _Wallet, value); }
        }
        Author _Author = new Author { BidStatus = BidStatus.None, Name = "amazon.com", ActiveDue = "Active due: 07/07/2022 (in 182 days)" };
        public Author Author
        {
            get { return _Author; }
            set { SetProperty(ref _Author, value); }
        }
    }
}