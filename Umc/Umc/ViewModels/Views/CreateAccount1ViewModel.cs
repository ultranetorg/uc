using UC.Umc.Common.Constants;

namespace UC.Umc.ViewModels.Views;

public partial class CreateAccount1ViewModel : BaseAccountViewModel
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

    public CreateAccount1ViewModel(INotificationsService notificationService, ILogger<CreateAccount1ViewModel> logger) : base(notificationService, logger)
    {
	}

	[RelayCommand]
    private void Randomize()
    {
        Password = CommonHelper.GenerateUniqueId(8);
    }

	[RelayCommand]
    private void TextChanged()
    {
		if (!string.IsNullOrEmpty(Password))
		{
			CharCountDone = Password.Length >= 8 && Password.Length <= 14;
			BothCasesDone = Password.Any(char.IsUpper) && Password.Any(char.IsLower);
			NumbersIncluded = Password.Any(char.IsNumber);

			SpecialCharacterIncluded = false;
			foreach (char ch in TextConstants.SPECIAL_CHARACTERS)
			{
				if (Password.Contains(ch))
				{
					SpecialCharacterIncluded = true;
				}
			}
		}
    }
}
