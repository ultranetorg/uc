using UC.Umc.Models;
using UC.Umc.Pages.Transactions;
using UC.Umc.Popups;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Dashboard;

public partial class EnterPinViewModel : BaseViewModel
{
	[ObservableProperty]
	private AccountModel _account;

	public EnterPinViewModel(ILogger<EnterPinViewModel> logger) : base(logger)
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
