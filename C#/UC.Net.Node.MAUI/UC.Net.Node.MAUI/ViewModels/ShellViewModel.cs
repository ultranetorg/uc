using UC.Net.Node.MAUI.Common.Constants;

namespace UC.Net.Node.MAUI.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    public AppSection Dashboard { get; set; }
    public AppSection AccountDetails { get; set; }

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
	}
}
