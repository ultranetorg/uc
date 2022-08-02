namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AuthorRegistrationViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public AuthorRegistrationViewModel(ILogger<AuthorRegistrationViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async void ClosePageAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}
