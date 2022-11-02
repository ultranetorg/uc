namespace UC.Umc.ViewModels;

public partial class AuthorRegistrationViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public AuthorRegistrationViewModel(ILogger<AuthorRegistrationViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task ClosePageAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}
