using UC.Umc.Models;
using UC.Umc.Pages.Transactions;
using UC.Umc.Popups;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Dashboard;

public partial class EnterPinBViewModel : BaseViewModel
{
	[ObservableProperty]
	private AccountModel _account;

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