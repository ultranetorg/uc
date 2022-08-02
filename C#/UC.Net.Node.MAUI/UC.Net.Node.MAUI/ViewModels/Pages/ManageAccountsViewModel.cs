namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ManageAccountsViewModel : BaseViewModel
{	
	[ObservableProperty]
    private Wallet _selectedItem;

	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();

    public ManageAccountsViewModel(ILogger<ManageAccountsViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	[RelayCommand]
    private async void CreateAsync()
    {
        await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
    }

	[RelayCommand]
    private async void RestoreAsync()
    {
        await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
    }

	[RelayCommand]
    private async void ItemTappedAsync(Wallet wallet)
    {
        if (wallet == null) return;

        await Shell.Current.Navigation.PushAsync(new AccountDetailsPage(wallet));
    }

	[RelayCommand]
    private async void OptionsAsync(Wallet wallet)
    {
        await AccountOptionsPopup.Show(wallet);
    }

	private void FillFakeData()
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
	}
}
