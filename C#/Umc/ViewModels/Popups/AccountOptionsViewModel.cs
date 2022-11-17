namespace UC.Umc.ViewModels.Popups;

public partial class AccountOptionsViewModel : BaseViewModel
{
	[ObservableProperty]
    public AccountViewModel _account;

	public AccountOptionsViewModel(ILogger<AccountOptionsViewModel> logger) : base(logger)
	{
	}

	[RelayCommand]
	private async Task Send()
	{
		await Shell.Current.Navigation.PushAsync(new SendPage());
		ClosePopup();
	}
}
