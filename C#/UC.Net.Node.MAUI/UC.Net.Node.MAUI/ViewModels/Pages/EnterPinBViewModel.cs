namespace UC.Net.Node.MAUI.ViewModels.Pages;

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
        await DeleteAccountPopup.Show(Account);
    }
	
    [RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}