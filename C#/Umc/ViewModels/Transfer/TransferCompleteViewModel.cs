namespace UC.Umc.ViewModels;

public partial class TransferCompleteViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

    public TransferCompleteViewModel(ILogger<TransferCompleteViewModel> logger) : base(logger)
    {
		LoadData();
    }

	private void LoadData()
	{
		Account = DefaultDataMock.CreateAccount();
	}
	
	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    private async Task DeleteAsync()
    {
        await DeleteAccountPopup.Show(Account);
    }
}