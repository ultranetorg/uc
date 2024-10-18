using UC.Umc.Pages.Account;
using UC.Umc.Pages.Dashboard;
using UC.Umc.Pages.Domains;
using UC.Umc.Pages.Resources;
using UC.Umc.Pages.Transactions;

namespace UC.Umc.Common.Constants;

internal static class Routes
{
	public const string DASHBOARD = nameof(DashboardPage);
	public const string ENTER_PINCODE = nameof(EnterPinPage);
	public const string NETWORK = nameof(NetworkPage);
	public const string HELP = nameof(HelpPage);
	public const string HELP_DETAILS = nameof(HelpDetailsPage);
	public const string ABOUT = nameof(AboutPage);
	public const string WHATS_NEW = nameof(WhatsNewPage);
	public const string SETTINGS = nameof(SettingsPage);

	public const string ACCOUNTS = nameof(ManageAccountsPage);
	public const string ACCOUNT_DETAILS = nameof(AccountDetailsPage);
	public const string CREATE_ACCOUNT = nameof(CreateAccountPage);
	public const string RESTORE_ACCOUNT = nameof(RestoreAccountPage);
	public const string DELETE_ACCOUNT = nameof(DeleteAccountPage);
	public const string PRIVATE_KEY = nameof(PrivateKeyPage);

	public const string DOMAINS = nameof(DomainsPage);
	public const string DOMAIN_DETAILS = nameof(DomainDetailsPage);
	public const string DOMAIN_REGISTRATION = nameof(DomainRegistrationPage);
	public const string DOMAIN_RENEWAL = nameof(DomainRenewalPage);
	public const string DOMAIN_TRANSFER = nameof(DomainTransferPage);
	public const string MAKE_BID = nameof(MakeBidPage);
	public const string RESOURCES = nameof(ResourcesPage);
	public const string RESOURCE_SEARCH = nameof(ResourcesSearchPage);
	public const string RESOURCE_REGISTRATION = nameof(ResourceRegistrationPage);
	public const string RESOURCE_TRANSFER = nameof(ResourceTransferPage);

	public const string SEND = nameof(SendPage);
	public const string TRANSACTIONS = nameof(TransactionsPage);
	public const string TRANSFER = nameof(ETHTransferPage);
	public const string COMPLETED_TRANSFERS = nameof(TransferCompletePage);
	public const string UNFINISHED_TRANSFERS = nameof(UnfinishTransferPage);
}
