namespace UC.Umc.ViewModels;

public partial class EnterPinViewModel : BasePageViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

    public EnterPinViewModel(INotificationsService notificationService, ILogger<EnterPinViewModel> logger) : base(notificationService, logger)
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
		await ShowPopup(new DeleteAccountPopup(Account));
	}

	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}
