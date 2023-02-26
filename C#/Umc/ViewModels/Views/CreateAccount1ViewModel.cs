namespace UC.Umc.ViewModels.Views;

public partial class CreateAccount1ViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    private string _password;

	[ObservableProperty]
    private bool _charCountDone = true;

	[ObservableProperty]
    private bool _bothCasesDone;

	[ObservableProperty]
    private bool _numbersIncluded;

	[ObservableProperty]
    private bool _specialCharacterIncluded;

    public CreateAccount1ViewModel(ILogger<CreateAccount1ViewModel> logger) : base(logger)
    {
	}

	[RelayCommand]
    private void Randomize()
    {
        Password = CommonHelper.GenerateUniqueId(8);
    }
}
