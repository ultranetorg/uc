namespace UC.Umc.ViewModels.Views;

public partial class AuthorRenewal1ViewModel : BaseViewModel
{
	[ObservableProperty]
	private AccountViewModel _account;

	public AuthorRenewal1ViewModel(ILogger<AuthorRenewal1ViewModel> logger): base(logger)
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