namespace UC.Umc.ViewModels;

public partial class SettingsViewModel : BasePageViewModel
{
	[ObservableProperty]
    private CustomCollection<string> _months;
    
	[ObservableProperty]
    private AccountViewModel _account;

    public SettingsViewModel(INotificationsService notificationService, ILogger<SettingsViewModel> logger) : base(notificationService, logger)
    {
		LoadData();
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();

	private void LoadData()
	{
		_months = DefaultDataMock.MonthList1;
		Account = DefaultDataMock.CreateAccount();
	}
}