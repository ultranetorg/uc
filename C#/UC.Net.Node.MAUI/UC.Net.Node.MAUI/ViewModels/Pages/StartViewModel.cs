namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class StartViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
    private Author _author = DefaultDataMock.Author1;

	public StartViewModel(ILogger<StartViewModel> logger) : base(logger)
	{
		Account = DefaultDataMock.CreateAccount();
	}

	[RelayCommand]
	private async Task StartAsync(string commandParameter)
	{
		// To be reworked
		switch(int.Parse(commandParameter))
        {
            case 0:
                await Shell.Current.Navigation.PushAsync(new AccountDetailsPage(Account, Ioc.Default.GetService<AccountDetailsViewModel>()));
                break;
            case 1:
                await Shell.Current.Navigation.PushAsync(new AuthorsPage(Ioc.Default.GetService<AuthorsViewModel>()));
                break;
            case 2:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchPage(Author, Ioc.Default.GetService<AuthorSearchPViewModel>()));
                break;
            case 19:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchBPage(Author, Ioc.Default.GetService<AuthorSearchBViewModel>()));
                break;
            case 20:
                await Shell.Current.Navigation.PushAsync(new AuthorSearchCPage(Author, Ioc.Default.GetService<AuthorSearchCViewModel>()));
                break;
            case 3:
                await Shell.Current.Navigation.PushAsync(new AuthorRegistrationPage(Ioc.Default.GetService<AuthorRegistrationViewModel>()));
                break;
            case 4:
                await Shell.Current.Navigation.PushAsync(new AuthorRenewalPage(Ioc.Default.GetService<AuthorRenewalViewModel>()));
                break;
            case 5:
                await Shell.Current.Navigation.PushAsync(new CreateAccountPage(Ioc.Default.GetService<CreateAccountPageViewModel>()));
                break;
            case 6:
                await Shell.Current.Navigation.PushAsync(new DashboardPage(Ioc.Default.GetService<DashboardViewModel>()));
                break;
            case 7:
                await Shell.Current.Navigation.PushAsync(new DeleteAccountPage(Account, Ioc.Default.GetService<DeleteAccountViewModel>()));
                break;
            case 8:
                await Shell.Current.Navigation.PushAsync(new ETHTransferPage(Ioc.Default.GetService<ETHTransferViewModel>()));
                break;
            case 9:
                await Shell.Current.Navigation.PushAsync(new MakeBidPage(Ioc.Default.GetService<MakeBidViewModel>()));
                break;
            case 10:
                await Shell.Current.Navigation.PushAsync(new ManageAccountsPage(Ioc.Default.GetService<ManageAccountsViewModel>()));
                break;
            case 11:
                await Shell.Current.Navigation.PushAsync(new PrivateKeyPage(Account, Ioc.Default.GetService<PrivateKeyViewModel>()));
                break;
            case 12:
                await Shell.Current.Navigation.PushAsync(new ProductSearchPage(Ioc.Default.GetService<ProductSearchViewModel>()));
                break;
            case 13:
                await Shell.Current.Navigation.PushAsync(new ProductsPage(Ioc.Default.GetService<ProductsViewModel>()));
                break;
            case 21:
                await Shell.Current.Navigation.PushAsync(new ProductsBPage(Ioc.Default.GetService<ProductsBViewModel>()));
                break;
            case 14:
                await Shell.Current.Navigation.PushAsync(new RestoreAccountPage(Ioc.Default.GetService<RestoreAccountViewModel>()));
                break;
            case 15:
                await Shell.Current.Navigation.PushAsync(new SendPage(Ioc.Default.GetService<SendViewModel>()));
                break;
            case 16:
                await Shell.Current.Navigation.PushAsync(new TransactionsPage(Ioc.Default.GetService<TransactionsViewModel>()));
                break;
            case 22:
                await Shell.Current.Navigation.PushAsync(new TransactionsBPage(Ioc.Default.GetService<TransactionsBViewModel>()));
                break;
            case 17:
                await Shell.Current.Navigation.PushAsync(new TransferCompletePage(Ioc.Default.GetService<TransferCompleteViewModel>()));
                break;
            case 18:
                await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage(Ioc.Default.GetService<UnfinishTransferViewModel>()));
                break;
            case 23:
                await Shell.Current.Navigation.PushAsync(new EnterPinPage(Ioc.Default.GetService<EnterPinViewModel>()));
                break;
            case 24:
                await Shell.Current.Navigation.PushAsync(new EnterPinBPage(Ioc.Default.GetService<EnterPinBViewModel>()));
                break;
            case 25:
                await CreatePinPopup.Show();
                break;
            case 26:
                await Shell.Current.Navigation.PushAsync(new NetworkPage(Ioc.Default.GetService<NetworkViewModel>()));
                break;
            case 27:
                await Shell.Current.Navigation.PushAsync(new SettingsPage(Ioc.Default.GetService<SettingsViewModel>()));
                break;
            case 28:
                await Shell.Current.Navigation.PushAsync(new SettingsBPage(Ioc.Default.GetService<SettingsBViewModel>()));
                break;
            case 29:
                await Shell.Current.Navigation.PushAsync(new HelpPage(Ioc.Default.GetService<HelpViewModel>()));
                break;
            case 30:
                await Shell.Current.Navigation.PushAsync(new HelpDetailsPage(Ioc.Default.GetService<HelpDetailsViewModel>()));
                break;
            case 31:
                await Shell.Current.Navigation.PushAsync(new WhatsNewPage(Ioc.Default.GetService<WhatsNewViewModel>()));
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
                await Shell.Current.Navigation.PushAsync(new AboutPage(Ioc.Default.GetService<AboutViewModel>()));
                break;
        }
	}
}
