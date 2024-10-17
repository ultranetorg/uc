using System.Collections.ObjectModel;
using UC.Umc.Models;
using UC.Umc.Pages.Transactions;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Dashboard;

public partial class SettingsBViewModel : BaseViewModel
{
	[ObservableProperty]
	private ObservableCollection<string> _months;
	
	[ObservableProperty]
	private AccountModel _account;

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
