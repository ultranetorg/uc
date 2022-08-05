namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class StartViewModel : BaseWalletViewModel
{
	public StartViewModel(ILogger<StartViewModel> logger) : base(logger)
	{
		Wallet = new()
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "2T52",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#bb50dd"),
		};
	}

	[RelayCommand]
	private async void StartAsync(string commandParameter)
	{
		switch(int.Parse(commandParameter))
        {
            case 0:
                await Shell.Current.Navigation.PushAsync(new AccountDetailsPage(Wallet));
                break;
            case 1:
                await Shell.Current.Navigation.PushAsync(new AuthorsPage());
                break;
            case 2:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchPage(Author));
                break;
            case 19:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchBPage(Author));
                break;
            case 20:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchCPage(Author));
                break;
            case 3:
                await Shell.Current.Navigation.PushAsync(new AuthorRegistrationPage());
                break;
            case 4:
                await Shell.Current.Navigation.PushAsync(new AuthorRegistrationRenewalPage());
                break;
            case 5:
                await Shell.Current.Navigation.PushAsync(new CreateAccountPage());
                break;
            case 6:
                await Shell.Current.Navigation.PushAsync(new DashboardPage());
                break;
            case 7:
                await Shell.Current.Navigation.PushAsync(new DeleteAccountPage(Wallet));
                break;
            case 8:
                await Shell.Current.Navigation.PushAsync(new ETHTransferPage());
                break;
            case 9:
                await Shell.Current.Navigation.PushAsync(new MakeBidPage());
                break;
            case 10:
                await Shell.Current.Navigation.PushAsync(new ManageAccountsPage());
                break;
            case 11:
                await Shell.Current.Navigation.PushAsync(new PrivateKeyPage(Wallet));
                break;
            case 12:
                await Shell.Current.Navigation.PushAsync(new ProductSearchPage());
                break;
            case 13:
                await Shell.Current.Navigation.PushAsync(new ProductsPage());
                break;
            case 21:
                await Shell.Current.Navigation.PushAsync(new ProductsBPage());
                break;
            case 14:
                await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
                break;
            case 15:
                await Shell.Current.Navigation.PushAsync(new SendPage());
                break;
            case 16:
                await Shell.Current.Navigation.PushAsync(new TransactionsPage());
                break;
            case 22:
                await Shell.Current.Navigation.PushAsync(new TransactionsBPage());
                break;
            case 17:
                await Shell.Current.Navigation.PushAsync(new TransferCompletePage());
                break;
            case 18:
                await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
                break;
            case 23:
                await Shell.Current.Navigation.PushAsync(new EnterPinPage());
                break;
            case 24:
                await Shell.Current.Navigation.PushAsync(new EnterPinBPage());
                break;
            case 25:
                await CreatePinPopup.Show();
                break;
            case 26:
                await Shell.Current.Navigation.PushAsync(new NetworkPage());
                break;
            case 27:
                await Shell.Current.Navigation.PushAsync(new SettingsPage());
                break;
            case 28:
                await Shell.Current.Navigation.PushAsync(new SettingsBPage());
                break;
            case 29:
                await Shell.Current.Navigation.PushAsync(new HelpPage());
                break;
            case 30:
                await Shell.Current.Navigation.PushAsync(new HelpBPage());
                break;
            case 31:
                await Shell.Current.Navigation.PushAsync(new WhatsNewPage());
                break;
            case 32:
                await WhatsNewPopup.Show();
                break;
            case 33:
                await NotificationsPopup.Show();
                break;
            case 34:
                await NotificationPopup.Show();
                break;
            case 35:
                await AccountsPopup.Show();
                break;
            case 36:
                await NoNetworkPopup.Show();
                break;
            case 37:
                await Shell.Current.Navigation.PushAsync(new AboutPage());
                break;
        }
	}
}
