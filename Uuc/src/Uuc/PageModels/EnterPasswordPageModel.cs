using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uuc.Pages;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class EnterPasswordPageModel
(
	IPasswordService passwordService,
	ISessionService sessionService,
	AppShell appShell
) : ObservableObject
{
	[ObservableProperty]
	private string _password = string.Empty;

	[RelayCommand]
	private async Task Submit_OnClicked()
	{
		bool isValid = await passwordService.IsValidAsync(Password);
		if (!isValid)
		{
			return;
		}

		passwordService.Password = Password;
		sessionService.StartSession();

		Application.Current.MainPage = appShell;
	}
}
