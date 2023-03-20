namespace UC.Umc.ViewModels;

public partial class EnterPinBViewModel : BasePageViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

    public EnterPinBViewModel(INotificationsService notificationService, ILogger<EnterPinBViewModel> logger) : base(notificationService, logger)
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