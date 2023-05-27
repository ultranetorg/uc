namespace UC.Umc.ViewModels;

public static class ViewModelExtensions
{
	public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
	{
		#region Pages

		// Singleton ViewModels
		builder.Services.AddSingleton<ShellViewModel>();

		builder.Services.AddSingleton<DashboardViewModel>();

		builder.Services.AddSingleton<ManageAccountsViewModel>();

		builder.Services.AddSingleton<AuthorsViewModel>();

		builder.Services.AddSingleton<ProductsViewModel>();

		builder.Services.AddSingleton<TransactionsViewModel>();

		// Transient ViewModels
		builder.Services.AddTransient<AccountDetailsViewModel>();

		builder.Services.AddTransient<PrivateKeyViewModel>();

		builder.Services.AddTransient<CreateAccountPageViewModel>();

		builder.Services.AddTransient<CreateAccount2ViewModel>();

		builder.Services.AddTransient<RestoreAccountViewModel>();

		builder.Services.AddTransient<DeleteAccountViewModel>();

		builder.Services.AddTransient<AuthorDetailsViewModel>();

		builder.Services.AddTransient<AuthorRegistrationViewModel>();

		builder.Services.AddTransient<AuthorRenewalViewModel>();

		builder.Services.AddTransient<AuthorTransferViewModel>();

		builder.Services.AddTransient<MakeBidViewModel>();

		builder.Services.AddTransient<ProductSearchViewModel>();

		builder.Services.AddTransient<ProductTransferViewModel>();

		builder.Services.AddTransient<ProductRegistrationViewModel>();

		builder.Services.AddTransient<SendViewModel>();

		builder.Services.AddTransient<ETHTransferViewModel>();

		builder.Services.AddTransient<TransferCompleteViewModel>();

		builder.Services.AddTransient<UnfinishTransferViewModel>();

		builder.Services.AddTransient<EnterPinViewModel>();

		builder.Services.AddTransient<CreatePinViewModel>();

		builder.Services.AddTransient<HelpDetailsViewModel>();

		builder.Services.AddTransient<NetworkViewModel>();

		builder.Services.AddTransient<SettingsViewModel>();

		builder.Services.AddTransient<AboutViewModel>();

		builder.Services.AddTransient<HelpViewModel>();

		builder.Services.AddTransient<WhatsNewViewModel>();

		builder.Services.AddTransient<NotificationsViewModel>();

		#endregion Pages

		#region Popups
		
		builder.Services.AddTransient<DeleteAccountPopupViewModel>();

		builder.Services.AddTransient<SourceAccountViewModel>();

		builder.Services.AddTransient<RecipientAccountViewModel>();

		builder.Services.AddTransient<AuthorOptionsViewModel>();

		builder.Services.AddTransient<SelectAuthorViewModel>();

		builder.Services.AddTransient<TransactionDetailsViewModel>();

		builder.Services.AddTransient<NotificationViewModel>();

		builder.Services.AddTransient<ProductOptionsViewModel>();

		builder.Services.AddTransient<TransferOptionsViewModel>();

		builder.Services.AddTransient<WhatsNewPopupViewModel>();

		#endregion

		return builder;
	}
}