namespace UC.Umc.ViewModels.Views;

public partial class AuthorRegistration1ViewModel : BaseViewModel
{
	[ObservableProperty]
	private AccountViewModel _account;

	public AuthorRegistration1ViewModel(ILogger<AuthorRegistration1ViewModel> logger): base(logger)
	{
	}

    [RelayCommand]
    private async Task SelectAccountAsync()
	{
		var popup = new SourceAccountPopup();
		await ShowPopup(popup);
		if (popup?.Vm?.Account != null)
		{
			Account = popup.Vm.Account;
		}
	}
}
