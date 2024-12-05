using CommunityToolkit.Diagnostics;

namespace Uuc.Services;

public class ApplicationService : IApplicationService
{
	public async Task DisplayAlert(string title, string message, string cancel)
	{
		Guard.IsNotEmpty(title);
		Guard.IsNotEmpty(message);

		if (Application.Current?.MainPage != null)
		{
			await Application.Current.MainPage.DisplayAlert(title, message, cancel);
		}
	}
}
