namespace UC.Umc.ViewModels.Views;

public partial class AuthorRegistration2ViewModel : BaseViewModel
{
	[ObservableProperty]
	private AccountViewModel _account;

	public AuthorRegistration2ViewModel(ILogger<AuthorRegistration2ViewModel> logger): base(logger)
	{
	}
}
