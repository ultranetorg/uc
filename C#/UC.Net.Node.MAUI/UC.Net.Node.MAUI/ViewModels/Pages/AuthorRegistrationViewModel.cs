namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AuthorRegistrationViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public AuthorRegistrationViewModel(Page page, ILogger<AuthorRegistrationViewModel> logger) : base(logger)
    {
        Page = page;
    }
}
