using CommunityToolkit.Maui;

namespace UC.Umc.ViewModels;

public static class ViewModelExtensions
{
	// Preliminary - to be reworked
	public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
	{
		#region Pages

		// Singleton ViewModels
		builder.Services.AddSingleton<ShellViewModel>();

		builder.Services.AddSingleton<ManageAccountsViewModel>();

		builder.Services.AddSingleton<DashboardViewModel>();

		builder.Services.AddSingleton<AuthorsViewModel>();

		builder.Services.AddSingleton<ProductSearchViewModel>();

		builder.Services.AddSingleton<TransactionsBViewModel>();

		builder.Services.AddSingleton<TransactionsViewModel>();

		// Transient ViewModels
		builder.Services.AddTransient<AccountDetailsViewModel>();

		builder.Services.AddTransient<AuthorRegistrationViewModel>();

		builder.Services.AddTransient<AuthorRenewalViewModel>();

		builder.Services.AddTransient<DeleteAccountViewModel>();

		builder.Services.AddTransient<RestoreAccountViewModel>();

		builder.Services.AddTransient<AuthorDetailsViewModel>();

		builder.Services.AddTransient<CreateAccountPageViewModel>();

		builder.Services.AddTransient<EnterPinBViewModel>();

		builder.Services.AddTransient<EnterPinViewModel>();

		builder.Services.AddTransient<ETHTransferViewModel>();

		builder.Services.AddTransient<HelpDetailsViewModel>();

		builder.Services.AddTransient<MakeBidViewModel>();

		builder.Services.AddTransient<NetworkViewModel>();

		builder.Services.AddTransient<PrivateKeyViewModel>();

		builder.Services.AddTransient<ProductsBViewModel>();

		builder.Services.AddTransient<ProductsViewModel>();

		builder.Services.AddTransient<SendViewModel>();

		builder.Services.AddTransient<SettingsBViewModel>();

		builder.Services.AddTransient<SettingsViewModel>();

		builder.Services.AddTransient<TransferCompleteViewModel>();

		builder.Services.AddTransient<UnfinishTransferViewModel>();

		builder.Services.AddTransient<AboutViewModel>();

		builder.Services.AddTransient<HelpViewModel>();

		builder.Services.AddTransient<WhatsNewViewModel>();

		#endregion Pages

		#region Popups

		// Transient ViewModels
		builder.Services.AddTransient<AccountOptionsViewModel>();
		builder.Services.AddTransient<AuthorOptionsViewModel>();
		builder.Services.AddTransient<DeleteAccountPopupViewModel>();
		builder.Services.AddTransient<NotificationsViewModel>();
		builder.Services.AddTransient<NotificationViewModel>();
		builder.Services.AddTransient<RecipientAccountViewModel>();
		builder.Services.AddTransient<SelectAuthorViewModel>();
		builder.Services.AddTransient<SourceAccountViewModel>();

		#endregion

		#region Views

		// Transient ViewModels
		builder.Services.AddTransient<AuthorRegistration1ViewModel>();
		builder.Services.AddTransient<AuthorRegistrationViewModel>();
		builder.Services.AddTransient<AuthorRenewalViewModel>();
		builder.Services.AddTransient<CreateAccountViewModel>();
		builder.Services.AddTransient<ETHTransfer2ViewModel>();
		builder.Services.AddTransient<ETHTransfer3ViewModel>();
		builder.Services.AddTransient<MakeBid1ViewModel>();
		builder.Services.AddTransient<MakeBid2ViewModel>();

		#endregion

		return builder;
	}
}