namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class StartViewModel : BaseWalletViewModel
{
	public StartViewModel(ILogger<StartViewModel> logger) : base(logger)
	{
		Wallet = DefaultDataMock.Wallet3;
	}

	[RelayCommand]
	private async void StartAsync(string commandParameter)
	{
		// To be reworked
		switch(int.Parse(commandParameter))
        {
            case 0:
                await Shell.Current.Navigation.PushAsync(new AccountDetailsPage(Wallet, App.ServiceProvider.GetService<AccountDetailsViewModel>()));
                break;
            case 1:
                await Shell.Current.Navigation.PushAsync(new AuthorsPage(App.ServiceProvider.GetService<AuthorsViewModel>()));
                break;
            case 2:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchPage(Author, App.ServiceProvider.GetService<AuthorSearchPViewModel>()));
                break;
            case 19:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchBPage(Author, App.ServiceProvider.GetService<AuthorSearchBViewModel>()));
                break;
            case 20:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchCPage(Author, App.ServiceProvider.GetService<AuthorSearchCViewModel>()));
                break;
            case 3:
                await Shell.Current.Navigation.PushAsync(new AuthorRegistrationPage(App.ServiceProvider.GetService<AuthorRegistrationViewModel>()));
                break;
            case 4:
                await Shell.Current.Navigation.PushAsync(new AuthorRenewalPage(App.ServiceProvider.GetService<AuthorRenewalViewModel>()));
                break;
            case 5:
                await Shell.Current.Navigation.PushAsync(new CreateAccountPage(App.ServiceProvider.GetService<CreateAccountPageViewModel>()));
                break;
            case 6:
                await Shell.Current.Navigation.PushAsync(new DashboardPage(App.ServiceProvider.GetService<DashboardViewModel>()));
                break;
            case 7:
                await Shell.Current.Navigation.PushAsync(new DeleteAccountPage(Wallet, App.ServiceProvider.GetService<DeleteAccountViewModel>()));
                break;
            case 8:
                await Shell.Current.Navigation.PushAsync(new ETHTransferPage(App.ServiceProvider.GetService<ETHTransferViewModel>()));
                break;
            case 9:
                await Shell.Current.Navigation.PushAsync(new MakeBidPage(App.ServiceProvider.GetService<MakeBidViewModel>()));
                break;
            case 10:
                await Shell.Current.Navigation.PushAsync(new ManageAccountsPage(App.ServiceProvider.GetService<ManageAccountsViewModel>()));
                break;
            case 11:
                await Shell.Current.Navigation.PushAsync(new PrivateKeyPage(Wallet, App.ServiceProvider.GetService<PrivateKeyViewModel>()));
                break;
            case 12:
                await Shell.Current.Navigation.PushAsync(new ProductSearchPage(App.ServiceProvider.GetService<ProductSearchViewModel>()));
                break;
            case 13:
                await Shell.Current.Navigation.PushAsync(new ProductsPage(App.ServiceProvider.GetService<ProductsViewModel>()));
                break;
            case 21:
                await Shell.Current.Navigation.PushAsync(new ProductsBPage(App.ServiceProvider.GetService<ProductsBViewModel>()));
                break;
            case 14:
                await Shell.Current.Navigation.PushAsync(new RestoreAccountPage(App.ServiceProvider.GetService<RestoreAccountViewModel>()));
                break;
            case 15:
                await Shell.Current.Navigation.PushAsync(new SendPage(App.ServiceProvider.GetService<SendViewModel>()));
                break;
            case 16:
                await Shell.Current.Navigation.PushAsync(new TransactionsPage(App.ServiceProvider.GetService<TransactionsViewModel>()));
                break;
            case 22:
                await Shell.Current.Navigation.PushAsync(new TransactionsBPage(App.ServiceProvider.GetService<TransactionsBViewModel>()));
                break;
            case 17:
                await Shell.Current.Navigation.PushAsync(new TransferCompletePage(App.ServiceProvider.GetService<TransferCompleteViewModel>()));
                break;
            case 18:
                await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage(App.ServiceProvider.GetService<UnfinishTransferViewModel>()));
                break;
            case 23:
                await Shell.Current.Navigation.PushAsync(new EnterPinPage(App.ServiceProvider.GetService<EnterPinViewModel>()));
                break;
            case 24:
                await Shell.Current.Navigation.PushAsync(new EnterPinBPage(App.ServiceProvider.GetService<EnterPinBViewModel>()));
                break;
            case 25:
                await CreatePinPopup.Show();
                break;
            case 26:
                await Shell.Current.Navigation.PushAsync(new NetworkPage(App.ServiceProvider.GetService<NetworkViewModel>()));
                break;
            case 27:
                await Shell.Current.Navigation.PushAsync(new SettingsPage(App.ServiceProvider.GetService<SettingsViewModel>()));
                break;
            case 28:
                await Shell.Current.Navigation.PushAsync(new SettingsBPage(App.ServiceProvider.GetService<SettingsBViewModel>()));
                break;
            case 29:
                await Shell.Current.Navigation.PushAsync(new HelpPage(App.ServiceProvider.GetService<HelpViewModel>()));
                break;
            case 30:
                await Shell.Current.Navigation.PushAsync(new HelpBPage(App.ServiceProvider.GetService<HelpBViewModel>()));
                break;
            case 31:
                await Shell.Current.Navigation.PushAsync(new WhatsNewPage(App.ServiceProvider.GetService<WhatsNewViewModel>()));
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
                await Shell.Current.Navigation.PushAsync(new AboutPage(App.ServiceProvider.GetService<AboutViewModel>()));
                break;
        }
	}
}
