namespace UC.Umc.ViewModels;

public partial class EnterPinViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

    public EnterPinViewModel(ILogger<EnterPinViewModel> logger) : base(logger)
    {
		LoadData();
    }

	private void LoadData()
	{
		Account = DefaultDataMock.CreateAccount();
	}

	[RelayCommand]
    private async Task DeleteAsync()
    {
        await DeleteAccountPopup.Show(Account);
    }

	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}
