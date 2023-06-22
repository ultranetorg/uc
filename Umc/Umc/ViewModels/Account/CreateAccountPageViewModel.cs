using UC.Umc.Common.Constants;

namespace UC.Umc.ViewModels;

public partial class CreateAccountPageViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    private string _password = string.Empty;

	[ObservableProperty]
    private bool _charCountDone;

	[ObservableProperty]
    private bool _bothCasesDone;

	[ObservableProperty]
    private bool _numbersIncluded;

	[ObservableProperty]
    private bool _specialCharacterIncluded;

    public CreateAccountPageViewModel(INotificationsService notificationService,
		ILogger<CreateAccountPageViewModel> logger) : base(notificationService, logger)
    {
    }

	internal async Task InitializeAsync()
	{
		Account = DefaultDataMock.CreateAccount(string.Empty);

		await Task.Delay(10);
	}

	[RelayCommand]
    private void Randomize()
    {
        Password = CommonHelper.GenerateUniqueId(CommonConstants.LENGTH_ACCOUNT);
    }

	[RelayCommand]
    private void TextChanged()
    {
		if (!string.IsNullOrEmpty(Password))
		{
			CharCountDone = Password.Length >= 8 && Password.Length <= 16;
			BothCasesDone = Password.Any(char.IsUpper) && Password.Any(char.IsLower);
			NumbersIncluded = Password.Any(char.IsNumber);

			SpecialCharacterIncluded = false;
			foreach (char ch in CommonConstants.SPECIAL_CHARACTERS)
			{
				if (Password.Contains(ch))
				{
					SpecialCharacterIncluded = true;
				}
			}
		}
    }

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		var firstStepValidation = CharCountDone && BothCasesDone && NumbersIncluded && SpecialCharacterIncluded;

		if (Position == 0 && firstStepValidation)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else if (Position == 1)
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully created!");
		}
	}
}