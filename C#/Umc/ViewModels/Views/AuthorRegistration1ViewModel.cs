namespace UC.Umc.ViewModels.Views;

public partial class AuthorRegistration1ViewModel : BaseViewModel
{
    public AuthorRegistration1ViewModel(ILogger<AuthorRegistration1ViewModel> logger): base(logger)
	{
	}

    [RelayCommand]
    private async Task SelectAccountAsync()
    {
        var account = await SourceAccountPopup.Show();
        if (account != null)
		{
			// Account = account;
		}
    }
}
