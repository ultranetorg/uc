namespace UC.Umc.ViewModels;

public partial class ShellViewModel : ObservableObject
{
	public AppSection Dashboard { get; set; }
	public AppSection Accounts { get; set; }
	public AppSection Authors { get; set; }
	public AppSection Products { get; set; }
	public AppSection ProductsSearch { get; set; }
	public AppSection Transactions { get; set; }
	public AppSection Transfer { get; set; }
	public AppSection Network { get; set; }
	public AppSection Settings { get; set; }
	public AppSection Help { get; set; }
	public AppSection About { get; set; }
	public AppSection WhatsNew { get; set; }

	public ShellViewModel()
	{
		Dashboard = new AppSection()
		{
			Title = nameof(Dashboard),
			Icon = "dashboard_outline.png",
			IconDark = string.Empty,
			TargetType = typeof(DashboardPage),
			Route = Routes.DASHBOARD
		};
		Accounts = new AppSection()
		{
			Title = nameof(Accounts),
			Icon = "accounts_light.png",
			IconDark = "accounts_dark.png",
			TargetType = typeof(ManageAccountsPage),
			Route = Routes.ACCOUNTS
		};
		Authors = new AppSection()
		{
			Title = nameof(Authors),
			Icon = "authors_light.png",
			IconDark = "authors_dark.png",
			TargetType = typeof(AuthorsPage),
			Route = Routes.AUTHORS
		};
		Products = new AppSection()
		{
			Title = nameof(Products),
			Icon = "products_light.png",
			IconDark = "products_dark.png",
			TargetType = typeof(ProductsPage),
			Route = Routes.PRODUCTS
		};
		ProductsSearch = new AppSection()
		{
			Title = "Products Search",
			Icon = "products_light.png",
			IconDark = "products_dark.png",
			TargetType = typeof(ProductSearchPage),
			Route = Routes.PRODUCT_SEARCH
		};
		Transactions = new AppSection()
		{
			Title = nameof(Transactions),
			Icon = "transactions_light.png",
			IconDark = "transactions_dark.png",
			TargetType = typeof(TransactionsPage),
			Route = Routes.TRANSACTIONS
		};
		Transfer = new AppSection()
		{
			Title = nameof(Transfer),
			Icon = "transfer_light.png",
			IconDark = "transfer_dark.png",
			TargetType = typeof(ETHTransferPage),
			Route = Routes.TRANSFER
		};
		Network = new AppSection()
		{
			Title = nameof(Network),
			Icon = "network_light.png",
			IconDark = "network_dark.png",
			TargetType = typeof(NetworkPage),
			Route = Routes.NETWORK
		};
		Settings = new AppSection()
		{
			Title = nameof(Settings),
			Icon = "settings.png",
			IconDark = "settings.png",
			TargetType = typeof(SettingsPage),
			Route = Routes.SETTINGS
		};
		Help = new AppSection()
		{
			Title = nameof(Help),
			Icon = "help_light.png",
			IconDark = "help_dark.png",
			TargetType = typeof(HelpPage),
			Route = Routes.HELP
		};
		About = new AppSection()
		{
			Title = nameof(About),
			Icon = "info_light.png",
			IconDark = "info_dark.png",
			TargetType = typeof(AboutPage),
			Route = Routes.ABOUT
		};
		WhatsNew = new AppSection()
		{
			Title = "Whats New",
			Icon = "info_light.png",
			IconDark = "info_dark.png",
			TargetType = typeof(WhatsNewPage),
			Route = Routes.WHATS_NEW
		};
	}
}
