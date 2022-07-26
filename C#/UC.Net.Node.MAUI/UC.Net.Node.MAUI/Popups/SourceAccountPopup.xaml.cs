using CommunityToolkit.Maui.Views;
using UC.Net.Node.MAUI.Controls;

namespace UC.Net.Node.MAUI.Popups
{
    public partial class SourceAccountPopup : Popup
    {
        private static SourceAccountPopup popup;
        SourceAccountViewModel viewModel;
        public SourceAccountPopup()
        {
            InitializeComponent();
            BindingContext = viewModel = new SourceAccountViewModel(this, ServiceHelper.GetService<ILogger<SourceAccountViewModel>>());
        }

        public void Hide()
        {
			Close();
        }

        //Show returns popup.viewModel.Wallet (Task<Wallet>)
    }
    public class SourceAccountViewModel : BaseViewModel
    {
        public SourceAccountViewModel(SourceAccountPopup popup, ILogger<SourceAccountViewModel> logger) : base(logger)
        {
            Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Unts = 5005,
                IconCode = "47F0",
                Name = "Main ultranet wallet",
                AccountColor = Color.FromArgb("#6601e3"),
            });
            Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Unts = 5005,
                IconCode = "2T52",
                Name = "Primary ultranet wallet",
                AccountColor = Color.FromArgb("#3765f4"),

            });
            Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Unts = 5005,
                IconCode = "9MDL",
                Name = "Secondary wallet",
                AccountColor = Color.FromArgb("#4cb16c"),

            });
            Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Unts = 5005,
                IconCode = "UYO3",
                Name = "Main ultranet wallet",
                AccountColor = Color.FromArgb("#e65c93"),

            });
            Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Unts = 5005,
                IconCode = "47FO",
                Name = "Main ultranet wallet",
                AccountColor = Color.FromArgb("#ba918c"),

            });
            Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Unts = 5005,
                IconCode = "2T52",
                Name = "Main ultranet wallet",
                AccountColor = Color.FromArgb("#d56a48"),

            });
            Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Unts = 5005,
                IconCode = "47FO",
                Name = "Main ultranet wallet",
                AccountColor = Color.FromArgb("#56d7de"),

            });
            Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Unts = 5005,
                IconCode = "2T52",
                Name = "Main ultranet wallet",
                AccountColor = Color.FromArgb("#bb50dd"),

            });
            Popup = popup;
        }
        public Command CloseCommad
        {
            get => new Command(Close);
        }

		// TODO: rework VM
        private void Close()
        {
           Popup.Hide();
        }
        CustomCollection<Wallet> _Wallets = new CustomCollection<Wallet>();
        public CustomCollection<Wallet> Wallets
        {
            get { return _Wallets; }
            set { SetProperty(ref _Wallets, value); }
        }
        public Page Page { get; }
        public Command ItemTppedCommand
        {
            get => new Command<Wallet>(ItemTpped);
        }
        private void ItemTpped(Wallet wallet)
        {
            foreach (var item in Wallets)
            {
                item.IsSelected = false;
            }
            wallet.IsSelected = true;
        }
        
        Wallet _Wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47F0",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#6601e3"),
        };
        public Wallet Wallet
        {
            get { return _Wallet; }
            set { SetProperty(ref _Wallet, value); }
        }

        public SourceAccountPopup Popup { get; }
    }

}