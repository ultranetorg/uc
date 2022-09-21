using UC.Net.Node.MAUI.Common.Constants;

namespace UC.Net.Node.MAUI.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    public AppSection Dashboard { get; set; }
    public AppSection AccountDetails { get; set; }
    public AppSection Authors { get; set; }
    public AppSection Products { get; set; }
    public AppSection Transactions { get; set; }
    public AppSection Network { get; set; }
    public AppSection Help { get; set; }

	public ShellViewModel()
	{
        Dashboard = new AppSection()
        {
            Title = "Dashboard",
            Icon = "dashboard_outline.png",
            IconDark = string.Empty,
            TargetType = typeof(DashboardPage),
            Route = ShellBaseRoutes.DASHBOARD
        };
		AccountDetails = new AppSection()
		{
			Title = "Account Details",
            Icon = "dotnet_bot.png",
            IconDark = string.Empty,
            TargetType = typeof(AccountDetailsPage),
            Route = ShellBaseRoutes.ACCOUNT_DETAILS
		};
		Authors = new AppSection()
		{
			Title = "Authors",
            Icon = "dotnet_bot.png",
            IconDark = string.Empty,
            TargetType = typeof(AuthorsPage),
            Route = ShellBaseRoutes.AUTHORS
		};
		Products = new AppSection()
		{
			Title = "Products",
            Icon = "dotnet_bot.png",
            IconDark = string.Empty,
            TargetType = typeof(ProductsPage),
            Route = ShellBaseRoutes.PRODUCTS
		};
		Transactions = new AppSection()
		{
			Title = "Transactions",
            Icon = "dotnet_bot.png",
            IconDark = string.Empty,
            TargetType = typeof(TransactionsPage),
            Route = ShellBaseRoutes.TRANSACTIONS
		};
		Network = new AppSection()
		{
			Title = "Network",
            Icon = "dotnet_bot.png",
            IconDark = string.Empty,
            TargetType = typeof(NetworkPage),
            Route = ShellBaseRoutes.NETWORK
		};
		Help = new AppSection()
		{
			Title = "Help",
            Icon = "dotnet_bot.png",
            IconDark = string.Empty,
            TargetType = typeof(HelpPage),
            Route = ShellBaseRoutes.HELP
		};
	}
}
