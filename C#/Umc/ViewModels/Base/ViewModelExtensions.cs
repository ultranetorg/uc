namespace UC.Umc.ViewModels;

public static class ViewModelExtensions
{
	// Preliminary - to be reworked
    public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
    {
		#region Pages

		// Singleton ViewModels
        builder.Services.AddSingleton(() => new ShellViewModel());
		builder.Services.AddSingleton(sp => new ManageAccountsViewModel(
			Ioc.Default.GetService<IAccountsService>(),
			Ioc.Default.GetService<ILogger<ManageAccountsViewModel>>()));

		builder.Services.AddSingleton(sp => new DashboardViewModel(
			Ioc.Default.GetService<ITransactionsService>(),
			Ioc.Default.GetService<IAccountsService>(),
			Ioc.Default.GetService<ILogger<DashboardViewModel>>()));

		builder.Services.AddSingleton(sp => new AuthorsViewModel(
			Ioc.Default.GetService<IAuthorsService>(),
			Ioc.Default.GetService<ILogger<AuthorsViewModel>>()));

		builder.Services.AddSingleton(sp => new ProductSearchViewModel(
			Ioc.Default.GetService<IProductsService>(),
			Ioc.Default.GetService<ILogger<ProductSearchViewModel>>()));

		builder.Services.AddSingleton(sp => new TransactionsBViewModel(
			Ioc.Default.GetService<ITransactionsService>(),
			Ioc.Default.GetService<ILogger<TransactionsBViewModel>>()));

		builder.Services.AddSingleton(sp => new TransactionsViewModel(
			Ioc.Default.GetService<ITransactionsService>(),
			Ioc.Default.GetService<ILogger<TransactionsViewModel>>()));

		// Transient ViewModels
		builder.Services.AddTransient(sp => new AccountDetailsViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<AccountDetailsViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorRegistrationViewModel(
			Ioc.Default.GetService<ILogger<AuthorRegistrationViewModel>>()));

		builder.Services.AddTransient(sp => new AuthorRenewalViewModel(
			Ioc.Default.GetService<ILogger<AuthorRenewalViewModel>>()));

		builder.Services.AddTransient(sp => new DeleteAccountViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<DeleteAccountViewModel>>()));

		builder.Services.AddTransient(sp => new RestoreAccountViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<RestoreAccountViewModel>>()));

		builder.Services.AddTransient(sp => new AuthorSearchBViewModel(
			Ioc.Default.GetService<ILogger<AuthorSearchBViewModel>>()));

		builder.Services.AddTransient(sp => new AuthorSearchCViewModel(
			Ioc.Default.GetService<ILogger<AuthorSearchCViewModel>>()));

		builder.Services.AddTransient(sp => new AuthorSearchPViewModel(
			Ioc.Default.GetService<ILogger<AuthorSearchPViewModel>>()));

		builder.Services.AddTransient(sp => new CreateAccountPageViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<CreateAccountPageViewModel>>()));

		builder.Services.AddTransient(sp => new EnterPinBViewModel(
			Ioc.Default.GetService<ILogger<EnterPinBViewModel>>()));

		builder.Services.AddTransient(sp => new EnterPinViewModel(
			Ioc.Default.GetService<ILogger<EnterPinViewModel>>()));

		builder.Services.AddTransient(sp => new ETHTransferViewModel(
			Ioc.Default.GetService<ILogger<ETHTransferViewModel>>()));

		builder.Services.AddTransient(sp => new HelpDetailsViewModel(
			Ioc.Default.GetService<ILogger<HelpDetailsViewModel>>()));

		builder.Services.AddTransient(sp => new MakeBidViewModel(
			Ioc.Default.GetService<ILogger<MakeBidViewModel>>()));

		builder.Services.AddTransient(sp => new NetworkViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<NetworkViewModel>>()));

		builder.Services.AddTransient(sp => new PrivateKeyViewModel(
			Ioc.Default.GetService<ILogger<PrivateKeyViewModel>>()));

		builder.Services.AddTransient(sp => new ProductsBViewModel(
			Ioc.Default.GetService<IProductsService>(),
			Ioc.Default.GetService<ILogger<ProductsBViewModel>>()));

		builder.Services.AddTransient(sp => new ProductsViewModel(
			Ioc.Default.GetService<IProductsService>(),
			Ioc.Default.GetService<ILogger<ProductsViewModel>>()));

		builder.Services.AddTransient(sp => new SendViewModel(
			Ioc.Default.GetService<ILogger<SendViewModel>>()));

		builder.Services.AddTransient(sp => new SettingsBViewModel(
			Ioc.Default.GetService<ILogger<SettingsBViewModel>>()));

		builder.Services.AddTransient(sp => new SettingsViewModel(
			Ioc.Default.GetService<ILogger<SettingsViewModel>>()));

		builder.Services.AddTransient(sp => new StartViewModel(
			Ioc.Default.GetService<ILogger<StartViewModel>>()));

		builder.Services.AddTransient(sp => new TransferCompleteViewModel(
			Ioc.Default.GetService<ILogger<TransferCompleteViewModel>>()));

		builder.Services.AddTransient(sp => new UnfinishTransferViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<UnfinishTransferViewModel>>()));

		builder.Services.AddTransient(sp => new AboutViewModel(
			Ioc.Default.GetService<ILogger<AboutViewModel>>()));

		builder.Services.AddTransient(sp => new HelpViewModel(
			Ioc.Default.GetService<ILogger<HelpViewModel>>()));

		builder.Services.AddTransient(sp => new WhatsNewViewModel(
			Ioc.Default.GetService<ILogger<WhatsNewViewModel>>()));

		#endregion Pages

		#region Popups

		// Transient ViewModels
		builder.Services.AddTransient(sp => new NotificationsViewModel(
			Ioc.Default.GetService<INotificationsService>(),
			Ioc.Default.GetService<ILogger<NotificationsViewModel>>()));
		builder.Services.AddTransient(sp => new NotificationViewModel(
			Ioc.Default.GetService<ILogger<NotificationViewModel>>()));
		builder.Services.AddTransient(sp => new RecipientAccountViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<RecipientAccountViewModel>>()));
		builder.Services.AddTransient(sp => new SelectAuthorViewModel(
			Ioc.Default.GetService<IAuthorsService>(),
			Ioc.Default.GetService<ILogger<SelectAuthorViewModel>>()));
		builder.Services.AddTransient(sp => new SourceAccountViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<SourceAccountViewModel>>()));

		#endregion

		#region Views

		// Transient ViewModels
		builder.Services.AddTransient(sp => new AuthorRegistration1ViewModel(
			Ioc.Default.GetService<ILogger<AuthorRegistration1ViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorRegistrationViewModel(
			Ioc.Default.GetService<ILogger<AuthorRegistrationViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorRenewal1ViewModel(
			Ioc.Default.GetService<ILogger<AuthorRenewal1ViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorRenewal2ViewModel(
			Ioc.Default.GetService<ILogger<AuthorRenewal2ViewModel>>()));
		builder.Services.AddTransient(sp => new CreateAccountViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<CreateAccountViewModel>>()));
		builder.Services.AddTransient(sp => new ETHTransfer2ViewModel(
			Ioc.Default.GetService<IServicesMockData>(),
			Ioc.Default.GetService<ILogger<ETHTransfer2ViewModel>>()));
		builder.Services.AddTransient(sp => new ETHTransfer3ViewModel(
			Ioc.Default.GetService<ILogger<ETHTransfer3ViewModel>>()));
		builder.Services.AddTransient(sp => new MakeBid1ViewModel(
			Ioc.Default.GetService<ILogger<MakeBid1ViewModel>>()));
		builder.Services.AddTransient(sp => new MakeBid2ViewModel(
			Ioc.Default.GetService<ILogger<MakeBid2ViewModel>>()));
		builder.Services.AddTransient(sp => new Send1ViewModel(
			Ioc.Default.GetService<ILogger<Send1ViewModel>>()));
		builder.Services.AddTransient(sp => new Send2ViewModel(
			Ioc.Default.GetService<ILogger<Send2ViewModel>>()));

		#endregion

		return builder;
    }
}
