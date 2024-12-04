using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uuc.PageModels.Base;
using Uuc.Pages;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class CreatePasswordPageModel(
	INavigationService navigationService,
	ISessionService sessionService,
	IPasswordService passwordService,
	AppShell appShell
) : BasePageModel(navigationService)
{
	[ObservableProperty]
	private string _name = string.Empty;

	[ObservableProperty]
	private string _password = string.Empty;

	[RelayCommand]
	private async Task Submit_OnClicked()
	{
		if (string.IsNullOrEmpty(Password))
		{
			return;
		}

		// await accountsService.CreateAsync(Password, Name);
		await passwordService.SaveHashAsync(Password);
		sessionService.StartSession();

		Application.Current.MainPage = appShell;
	}
}

