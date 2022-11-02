namespace UC.Umc.ViewModels;

public partial class AuthorRenewalViewModel : BaseAccountViewModel
{
    public AuthorRenewalViewModel(ILogger<AuthorRenewalViewModel> logger) : base(logger)
    {
    }

    [RelayCommand]
    private async Task SelectAuthorAsync()
    {
        var author = await SelectAuthorPopup.Show();
        if (author != null)
		{
			// Author = author;
		}
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