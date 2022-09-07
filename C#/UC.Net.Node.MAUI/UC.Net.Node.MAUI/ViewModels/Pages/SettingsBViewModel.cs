namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class SettingsBViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<string> _months;
    
	[ObservableProperty]
    private Wallet _wallet = new()
	{
		Id = Guid.NewGuid(),
		Unts = 5005,
		IconCode = "47F0",
		Name = "Main ultranet wallet",
		AccountColor = Color.FromArgb("#6601e3"),
	};

    public SettingsBViewModel(ILogger<SettingsBViewModel> logger) : base(logger)
    {
		LoadData();
    }

	[RelayCommand]
    private async void TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    private async void CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	private void LoadData()
	{
		_months = new();
        Months.Add("April");
        Months.Add("May");
        Months.Add("June");
        Months.Add("July");
        Months.Add("Augest");
        Months.Add("Spetemper");
        Months.Add("November");
	}
}
