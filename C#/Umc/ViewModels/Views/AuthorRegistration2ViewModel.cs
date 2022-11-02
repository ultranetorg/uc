namespace UC.Umc.ViewModels.Views;

public partial class AuthorRegistration2ViewModel : BaseViewModel
{
    public AuthorRegistration2ViewModel(ILogger<AuthorRegistration2ViewModel> logger): base(logger)
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
