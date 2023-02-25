namespace UC.Umc.ViewModels;

public partial class TransferCompleteViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(UntComission))]
	[NotifyPropertyChangedFor(nameof(EthComission))]
	private decimal _untAmount = 112;

	public decimal UntComission => (UntAmount + 1) / 10;
	public decimal EthComission => (UntAmount + 1) / 100;

	public string TransactionDate => "10/15/2021 19:24";

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
		await ShowPopup(new DeleteAccountPopup(Account));
	}
}