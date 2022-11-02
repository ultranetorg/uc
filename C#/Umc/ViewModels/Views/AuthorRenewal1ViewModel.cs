namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class AuthorRenewal1ViewModel : BaseViewModel
{
    public AuthorRenewal1ViewModel(ILogger<AuthorRenewal1ViewModel> logger): base(logger)
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