namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class SettingsBViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<string> _months;
    
	[ObservableProperty]
    private AccountViewModel _account;

    public SettingsBViewModel(ILogger<SettingsBViewModel> logger) : base(logger)
    {
		LoadData();
    }

	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	private void LoadData()
	{
		_months = DefaultDataMock.MonthList1;
		Account = DefaultDataMock.CreateAccount();
	}
}
