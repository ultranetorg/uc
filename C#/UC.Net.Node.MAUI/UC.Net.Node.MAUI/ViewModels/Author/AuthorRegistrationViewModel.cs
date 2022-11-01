namespace UC.Net.Node.MAUI.ViewModels;

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
