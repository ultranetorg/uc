namespace UC.Umc.ViewModels;

public partial class EnterPinBViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

    public EnterPinBViewModel(ILogger<EnterPinBViewModel> logger) : base(logger)
    {
		LoadData();
    }

	public void LoadData()
	{
		Account = DefaultDataMock.CreateAccount();
	}

    [RelayCommand]
    private async Task DeleteAsync()
    {
        await ShowPopup(new DeleteAccountPopup(Account));
    }
	
    [RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}