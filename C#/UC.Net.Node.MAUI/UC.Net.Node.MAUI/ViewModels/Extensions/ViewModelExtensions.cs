namespace UC.Net.Node.MAUI.ViewModels;

public static class ViewModelExtensions
{
	// Preliminary - to be reworked
    public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
    {
		#region Pages

		// Singleton ViewModels
        builder.Services.AddSingleton(() => new ShellViewModel());
		builder.Services.AddSingleton(sp => new AccountDetailsViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<AccountDetailsViewModel>>()));
		builder.Services.AddSingleton(sp => new AuthorRegistrationViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorRegistrationViewModel>>()));
		builder.Services.AddSingleton(sp => new AuthorRenewalViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorRenewalViewModel>>()));
		builder.Services.AddSingleton(sp => new DeleteAccountViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<DeleteAccountViewModel>>()));
		builder.Services.AddSingleton(sp => new ManageAccountsViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<ManageAccountsViewModel>>()));
		builder.Services.AddTransient(sp => new RestoreAccountViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<RestoreAccountViewModel>>()));

		// Transient ViewModels
		builder.Services.AddTransient(sp => new AboutViewModel(
			App.ServiceProvider.GetService<ILogger<AboutViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorSearchBViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorSearchBViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorSearchCViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorSearchCViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorSearchPViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorSearchPViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorsViewModel(
			App.ServiceProvider.GetService<IAuthorsService>(),
			App.ServiceProvider.GetService<ILogger<AuthorsViewModel>>()));
		builder.Services.AddTransient(sp => new CreateAccountPageViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<CreateAccountPageViewModel>>()));
		builder.Services.AddTransient(sp => new DashboardViewModel(
			App.ServiceProvider.GetService<ITransactionsService>(),
			App.ServiceProvider.GetService<ILogger<DashboardViewModel>>()));
		builder.Services.AddTransient(sp => new EnterPinBViewModel(
			App.ServiceProvider.GetService<ILogger<EnterPinBViewModel>>()));
		builder.Services.AddTransient(sp => new EnterPinViewModel(
			App.ServiceProvider.GetService<ILogger<EnterPinViewModel>>()));
		builder.Services.AddTransient(sp => new ETHTransferViewModel(
			App.ServiceProvider.GetService<ILogger<ETHTransferViewModel>>()));
		builder.Services.AddTransient(sp => new HelpViewModel(
			App.ServiceProvider.GetService<ILogger<HelpViewModel>>()));
		builder.Services.AddTransient(sp => new HelpBViewModel(
			App.ServiceProvider.GetService<ILogger<HelpBViewModel>>()));
		builder.Services.AddTransient(sp => new MakeBidViewModel(
			App.ServiceProvider.GetService<ILogger<MakeBidViewModel>>()));
		builder.Services.AddTransient(sp => new NetworkViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<NetworkViewModel>>()));
		builder.Services.AddTransient(sp => new PrivateKeyViewModel(
			App.ServiceProvider.GetService<ILogger<PrivateKeyViewModel>>()));
		builder.Services.AddTransient(sp => new ProductsBViewModel(
			App.ServiceProvider.GetService<IProductsService>(),
			App.ServiceProvider.GetService<ILogger<ProductsBViewModel>>()));
		builder.Services.AddTransient(sp => new ProductsViewModel(
			App.ServiceProvider.GetService<IProductsService>(),
			App.ServiceProvider.GetService<ILogger<ProductsViewModel>>()));
		builder.Services.AddTransient(sp => new ProductSearchViewModel(
			App.ServiceProvider.GetService<IProductsService>(),
			App.ServiceProvider.GetService<ILogger<ProductSearchViewModel>>()));
		builder.Services.AddTransient(sp => new SendViewModel(
			App.ServiceProvider.GetService<ILogger<SendViewModel>>()));
		builder.Services.AddTransient(sp => new SettingsBViewModel(
			App.ServiceProvider.GetService<ILogger<SettingsBViewModel>>()));
		builder.Services.AddTransient(sp => new SettingsViewModel(
			App.ServiceProvider.GetService<ILogger<SettingsViewModel>>()));
		builder.Services.AddTransient(sp => new StartViewModel(
			App.ServiceProvider.GetService<ILogger<StartViewModel>>()));
		builder.Services.AddTransient(sp => new TransactionsBViewModel(
			App.ServiceProvider.GetService<ITransactionsService>(),
			App.ServiceProvider.GetService<ILogger<TransactionsBViewModel>>()));
		builder.Services.AddTransient(sp => new TransactionsViewModel(
			App.ServiceProvider.GetService<ITransactionsService>(),
			App.ServiceProvider.GetService<ILogger<TransactionsViewModel>>()));
		builder.Services.AddTransient(sp => new TransferCompleteViewModel(
			App.ServiceProvider.GetService<ILogger<TransferCompleteViewModel>>()));
		builder.Services.AddTransient(sp => new UnfinishTransferViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<UnfinishTransferViewModel>>()));
		builder.Services.AddTransient(sp => new WhatsNewViewModel(
			App.ServiceProvider.GetService<ILogger<WhatsNewViewModel>>()));

		#endregion Pages

		#region Popups

		// Transient ViewModels
		builder.Services.AddTransient(sp => new NotificationsViewModel(
			App.ServiceProvider.GetService<INotificationsService>(),
			App.ServiceProvider.GetService<ILogger<NotificationsViewModel>>()));
		builder.Services.AddTransient(sp => new NotificationViewModel(
			App.ServiceProvider.GetService<ILogger<NotificationViewModel>>()));
		builder.Services.AddTransient(sp => new RecipientAccountViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<RecipientAccountViewModel>>()));
		builder.Services.AddTransient(sp => new SelectAuthorViewModel(
			App.ServiceProvider.GetService<IAuthorsService>(),
			App.ServiceProvider.GetService<ILogger<SelectAuthorViewModel>>()));
		builder.Services.AddTransient(sp => new SourceAccountViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<SourceAccountViewModel>>()));

		#endregion

		#region Views

		// Transient ViewModels
		builder.Services.AddTransient(sp => new AuthorRegistration1ViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorRegistration1ViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorRegistrationViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorRegistrationViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorRenewal1ViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorRenewal1ViewModel>>()));
		builder.Services.AddTransient(sp => new AuthorRenewal2ViewModel(
			App.ServiceProvider.GetService<ILogger<AuthorRenewal2ViewModel>>()));
		builder.Services.AddTransient(sp => new CreateAccountViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<CreateAccountViewModel>>()));
		builder.Services.AddTransient(sp => new ETHTransfer2ViewModel(
			App.ServiceProvider.GetService<IServicesMockData>(),
			App.ServiceProvider.GetService<ILogger<ETHTransfer2ViewModel>>()));
		builder.Services.AddTransient(sp => new ETHTransfer3ViewModel(
			App.ServiceProvider.GetService<ILogger<ETHTransfer3ViewModel>>()));
		builder.Services.AddTransient(sp => new MakeBid1ViewModel(
			App.ServiceProvider.GetService<ILogger<MakeBid1ViewModel>>()));
		builder.Services.AddTransient(sp => new MakeBid2ViewModel(
			App.ServiceProvider.GetService<ILogger<MakeBid2ViewModel>>()));
		builder.Services.AddTransient(sp => new Send1ViewModel(
			App.ServiceProvider.GetService<ILogger<Send1ViewModel>>()));
		builder.Services.AddTransient(sp => new Send2ViewModel(
			App.ServiceProvider.GetService<ILogger<Send2ViewModel>>()));

		#endregion

		return builder;
    }
}
