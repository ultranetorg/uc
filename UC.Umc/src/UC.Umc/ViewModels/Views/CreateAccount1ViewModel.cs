using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.ViewModels.Accounts;

namespace UC.Umc.ViewModels.Views;

public partial class CreateAccount1ViewModel(ILogger<CreateAccount1ViewModel> logger) : BaseAccountViewModel(logger)
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
