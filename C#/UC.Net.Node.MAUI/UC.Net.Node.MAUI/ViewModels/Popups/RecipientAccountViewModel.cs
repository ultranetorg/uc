namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class RecipientAccountViewModel : BaseViewModel
{
    public RecipientAccountPopup Popup { get; }

	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();
        
	[ObservableProperty]
    private Wallet _wallet = new()
	{
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };

    public RecipientAccountViewModel(RecipientAccountPopup popup, ILogger<NotificationsViewModel> logger): base(logger)
    {
		AddFakeData();
        Popup = popup;
    }

	[RelayCommand]
    private void Close()
    {
        Popup.Hide();
    }

	[RelayCommand]
    private void ItemTapped(Wallet wallet)
    {
        foreach (var item in Wallets)
        {
            item.IsSelected = false;
        }
        wallet.IsSelected = true;
    }

	private void AddFakeData()
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
            AccountColor = Color.FromArgb("#3765f4")
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "9MDL",
            Name = "Secondary wallet",
            AccountColor = Color.FromArgb("#4cb16c")
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "UYO3",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#e65c93")
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47FO",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#ba918c")
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "2T52",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#d56a48")
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47FO",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#56d7de")
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "2T52",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#bb50dd")
        });
	}
}
