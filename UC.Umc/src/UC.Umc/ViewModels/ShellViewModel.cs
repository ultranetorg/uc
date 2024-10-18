using UC.Umc.Common.Constants;
using UC.Umc.Models.Common;
using UC.Umc.Pages.Account;
using UC.Umc.Pages.Dashboard;
using UC.Umc.Pages.Domains;
using UC.Umc.Pages.Resources;
using UC.Umc.Pages.Transactions;

namespace UC.Umc.ViewModels;

public partial class ShellViewModel : ObservableObject
{
	public AppSection Dashboard { get; set; } = new()
	{
		Title = nameof(Dashboard),
		Icon = "dashboard_outline.png",
		IconDark = string.Empty,
		TargetType = typeof(DashboardPage),
		Route = Routes.DASHBOARD
	};

	public AppSection Accounts { get; set; } = new()
	{
		Title = nameof(Accounts),
		Icon = "accounts_light.png",
		IconDark = "accounts_dark.png",
		TargetType = typeof(ManageAccountsPage),
		Route = Routes.ACCOUNTS
	};

	public AppSection Domains { get; set; } = new()
	{
		Title = nameof(Domains),
		Icon = "authors_light.png",
		IconDark = "authors_dark.png",
		TargetType = typeof(DomainsPage),
		Route = Routes.DOMAINS
	};

	public AppSection Resources { get; set; } = new()
	{
		Title = nameof(Resources),
		Icon = "products_light.png",
		IconDark = "products_dark.png",
		TargetType = typeof(ResourcesPage),
		Route = Routes.RESOURCES
	};

	public AppSection ProductsSearch { get; set; } = new()
	{
		Title = $"{nameof(Resources)} Search",
		Icon = "products_light.png",
		IconDark = "products_dark.png",
		TargetType = typeof(ResourcesSearchPage),
		Route = Routes.RESOURCE_SEARCH
	};

	public AppSection Transactions { get; set; } = new()
	{
		Title = nameof(Transactions),
		Icon = "transactions_light.png",
		IconDark = "transactions_dark.png",
		TargetType = typeof(TransactionsPage),
		Route = Routes.TRANSACTIONS
	};

	public AppSection Transfer { get; set; } = new()
	{
		Title = nameof(Transfer),
		Icon = "transfer_light.png",
		IconDark = "transfer_dark.png",
		TargetType = typeof(ETHTransferPage),
		Route = Routes.TRANSFER
	};

	public AppSection Network { get; set; } = new()
	{
		Title = nameof(Network),
		Icon = "network_light.png",
		IconDark = "network_dark.png",
		TargetType = typeof(NetworkPage),
		Route = Routes.NETWORK
	};

	public AppSection Settings { get; set; } = new()
	{
		Title = nameof(Settings),
		Icon = "settings.png",
		IconDark = "settings.png",
		TargetType = typeof(SettingsPage),
		Route = Routes.SETTINGS
	};

	public AppSection Help { get; set; } = new()
	{
		Title = nameof(Help),
		Icon = "help_light.png",
		IconDark = "help_dark.png",
		TargetType = typeof(HelpPage),
		Route = Routes.HELP
	};

	public AppSection About { get; set; } = new()
	{
		Title = nameof(About),
		Icon = "info_light.png",
		IconDark = "info_dark.png",
		TargetType = typeof(AboutPage),
		Route = Routes.ABOUT
	};

	public AppSection WhatsNew { get; set; } = new()
	{
		Title = "Whats New",
		Icon = "info_light.png",
		IconDark = "info_dark.png",
		TargetType = typeof(WhatsNewPage),
		Route = Routes.WHATS_NEW
	};
}
