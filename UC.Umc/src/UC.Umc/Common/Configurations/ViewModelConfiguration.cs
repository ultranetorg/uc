using UC.Umc.ViewModels;
using UC.Umc.ViewModels.Accounts;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.ViewModels.Domains;
using UC.Umc.ViewModels.Popups;
using UC.Umc.ViewModels.Resources;
using UC.Umc.ViewModels.Transactions;
using UC.Umc.ViewModels.Views;

namespace UC.Umc.Common.Configurations;

public static class ViewModelExtensions
{
	// Preliminary - to be reworked
	public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
	{
		// ViewModels.
		builder.Services.AddSingleton<ShellViewModel>();
		builder.Services.AddSingleton<DashboardViewModel>();
		builder.Services.AddSingleton<DomainsViewModel>();
		builder.Services.AddSingleton<AccountsViewModel>();
		builder.Services.AddSingleton<ResourceViewModel>();
		builder.Services.AddSingleton<TransactionsViewModel>();
		builder.Services.AddSingleton<TransactionsViewModel>();

		// ViewModels.
		builder.Services.AddTransient<AboutViewModel>();
		builder.Services.AddTransient<AccountDetailsViewModel>();
		builder.Services.AddTransient<CreateAccount1ViewModel>();
		builder.Services.AddTransient<CreateAccount2ViewModel>();
		builder.Services.AddTransient<CreateAccountPageViewModel>();
		builder.Services.AddTransient<DeleteAccountViewModel>();
		builder.Services.AddTransient<DomainDetailsViewModel>();
		builder.Services.AddTransient<DomainRegistrationViewModel>();
		builder.Services.AddTransient<DomainRenewalViewModel>();
		builder.Services.AddTransient<DomainTransferViewModel>();
		builder.Services.AddTransient<EnterPinBViewModel>();
		builder.Services.AddTransient<EnterPinViewModel>();
		builder.Services.AddTransient<ETHTransferViewModel>();
		builder.Services.AddTransient<HelpDetailsViewModel>();
		builder.Services.AddTransient<HelpViewModel>();
		builder.Services.AddTransient<MakeBidViewModel>();
		builder.Services.AddTransient<NetworkViewModel>();
		builder.Services.AddTransient<PrivateKeyViewModel>();
		builder.Services.AddTransient<ResourceListViewModel>();
		builder.Services.AddTransient<ResourceRegistrationViewModel>();
		builder.Services.AddTransient<ResourceSearchViewModel>();
		builder.Services.AddTransient<ResourceTransferViewModel>();
		builder.Services.AddTransient<RestoreAccountViewModel>();
		builder.Services.AddTransient<SendViewModel>();
		builder.Services.AddTransient<SettingsBViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<TransferCompleteViewModel>();
		builder.Services.AddTransient<UnfinishTransferViewModel>();
		builder.Services.AddTransient<WhatsNewViewModel>();

		// Popups.
		builder.Services.AddTransient<AccountOptionsViewModel>();
		builder.Services.AddTransient<DomainOptionsViewModel>();
		builder.Services.AddTransient<DeleteAccountPopupViewModel>();
		builder.Services.AddTransient<NotificationsViewModel>();
		builder.Services.AddTransient<NotificationViewModel>();
		builder.Services.AddTransient<ResourceOptionsViewModel>();
		builder.Services.AddTransient<RecipientAccountViewModel>();
		builder.Services.AddTransient<SelectDomainViewModel>();
		builder.Services.AddTransient<SourceAccountViewModel>();
		builder.Services.AddTransient<TransactionDetailsViewModel>();
		builder.Services.AddTransient<TransferOptionsViewModel>();
		builder.Services.AddTransient<WhatsNewPopupViewModel>();

		return builder;
	}
}
