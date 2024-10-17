using System.Collections.ObjectModel;
using UC.Umc.Common.Helpers;
using UC.Umc.Models.Common;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Accounts;

public partial class CreateAccountPageViewModel(IServicesMockData service, ILogger<CreateAccountPageViewModel> logger)
	: BaseAccountViewModel(logger)
{
	[ObservableProperty]
	private AccountColor _selectedAccountColor;

	internal async Task InitializeAsync()
	{
		ColorsCollection.Clear();
		ColorsCollection.AddRange(service.AccountColors);

		await Task.Delay(1);
	}

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		if (Position == 0)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully created!");
		}
	}
}