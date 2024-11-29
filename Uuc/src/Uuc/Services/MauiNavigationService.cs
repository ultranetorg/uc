using CommunityToolkit.Diagnostics;

namespace Uuc.Services;

public class MauiNavigationService : INavigationService
{
	public Task NavigateToAsync(string route, IDictionary<string, object>? routeParameters)
	{
		Guard.IsNotEmpty(route);

		ShellNavigationState shellNavigation = new (route);

		return routeParameters != null
			? Shell.Current.GoToAsync(shellNavigation, routeParameters)
			: Shell.Current.GoToAsync(shellNavigation);
	}

	public Task PopAsync()
	{
		return Shell.Current.GoToAsync("..");
	}
}
