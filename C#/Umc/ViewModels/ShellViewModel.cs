

namespace UC.Umc.ViewModels;

public partial class ShellViewModel : ObservableObject
{
	public AppSection Dashboard { get; set; }
	public AppSection Accounts { get; set; }
	public AppSection Authors { get; set; }
	public AppSection Products { get; set; }
	public AppSection Transactions { get; set; }
	public AppSection Transfer { get; set; }
	public AppSection Network { get; set; }
	public AppSection Help { get; set; }

	public ShellViewModel()
	{
		Dashboard = new AppSection()
		{
			Title = nameof(Dashboard),
			Icon = "dashboard_outline.png",
			IconDark = string.Empty,
			TargetType = typeof(DashboardPage),
			Route = ShellBaseRoutes.DASHBOARD
		};
		Accounts = new AppSection()
		{
			Title = nameof(Accounts),
			Icon = "accounts_light.png",
			IconDark = "accounts_dark.png",
			TargetType = typeof(ManageAccountsPage),
			Route = ShellBaseRoutes.ACCOUNTS
		};
		Authors = new AppSection()
		{
			Title = nameof(Authors),
			Icon = "authors_light.png",
			IconDark = "authors_dark.png",
			TargetType = typeof(AuthorsPage),
			Route = ShellBaseRoutes.AUTHORS
		};
		Products = new AppSection()
		{
			Title = nameof(Products),
			Icon = "products_light.png",
			IconDark = "products_dark.png",
			TargetType = typeof(ProductsPage),
			Route = ShellBaseRoutes.PRODUCTS
		};
		Transactions = new AppSection()
		{
			Title = nameof(Transactions),
			Icon = "transactions_light.png",
			IconDark = "transactions_dark.png",
			TargetType = typeof(TransactionsPage),
			Route = ShellBaseRoutes.TRANSACTIONS
		};
		Transfer = new AppSection()
		{
			Title = nameof(Transfer),
			Icon = "transfer_light.png",
			IconDark = "transfer_dark.png",
			TargetType = typeof(ETHTransferPage),
			Route = ShellBaseRoutes.TRANSFER
		};
		Network = new AppSection()
		{
			Title = nameof(Network),
			Icon = "network_light.png",
			IconDark = "network_dark.png",
			TargetType = typeof(NetworkPage),
			Route = ShellBaseRoutes.NETWORK
		};
		Help = new AppSection()
		{
			Title = nameof(Help),
			Icon = "help_light.png",
			IconDark = "help_dark.png",
			TargetType = typeof(HelpPage),
			Route = ShellBaseRoutes.HELP
		};
	}
}
