using CommunityToolkit.Diagnostics;

namespace Uuc.Services;

public class NavigationService : INavigationService
{
	public async void Push<T>(T page, IDictionary<string, object>? routeParameters) where T : Page
	{
		Guard.IsAssignableToType<Page>(page);

		if (Shell.Current != null)
		{
			string routeName = typeof(T).ToString();
			ShellNavigationState shellNavigation = new(routeName);

			if (routeParameters != null)
			{
				await Shell.Current.GoToAsync(shellNavigation, routeParameters);
			}

			await Shell.Current.GoToAsync(shellNavigation);
		}

		if (Application.Current?.MainPage != null)
		{
			await Application.Current.MainPage.Navigation.PushAsync(page);
		}
	}

	public async void Pop()
	{
		if (Shell.Current != null)
		{
			await Shell.Current.GoToAsync("..");
		}

		if (Application.Current?.Windows.Count > 0)
		{
			await Application.Current.Windows[0].Navigation.PopAsync();
		}
	}
}
