namespace UC.Umc.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<string> _months;
    
	[ObservableProperty]
    private AccountViewModel _account;

    public SettingsViewModel(ILogger<SettingsViewModel> logger) : base(logger)
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