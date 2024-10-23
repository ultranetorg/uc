using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Pages.Transactions;
using UC.Umc.Popups;
using UC.Umc.Services.Accounts;

namespace UC.Umc.ViewModels.Accounts;

public partial class AccountsViewModel(IAccountsService service, ILogger<AccountsViewModel> logger)
	: BaseViewModel(logger)
{
	[ObservableProperty]
	private ObservableCollection<AccountModel> _accounts = new();

	public async Task InitializeAsync()
	{
		var accounts = await service.ListAccountsAsync();
		Accounts = new(accounts);
	}

	[RelayCommand]
	private async Task OpenOptionsAsync(AccountModel account)
	{
		try
		{
			await ShowPopup(new AccountOptionsPopup(account));
		}
		catch(ArgumentException ex)
		{
			_logger.LogError("OpenOptionsAsync: Account cannot be null, Error: {Message}", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenOptionsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task OpenDetailsAsync(AccountModel account) => 
		await Navigation.GoToAsync(Routes.ACCOUNT_DETAILS, new Dictionary<string,object>()
		{
			{ QueryKeys.ACCOUNT, account }
		});

	[RelayCommand]
	private async Task ReceiveAsync(AccountModel account) =>
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, null },
			{ QueryKeys.RECIPIENT_ACCOUNT, account }
		});
	
	[RelayCommand]
	private async Task SendAsync(AccountModel account) =>
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, account },
			{ QueryKeys.RECIPIENT_ACCOUNT, null }
		});

	[RelayCommand]
	private async Task CreateAsync() => await Navigation.GoToAsync(Routes.CREATE_ACCOUNT);

	[RelayCommand]
	private async Task RestoreAsync() => await Navigation.GoToAsync(Routes.RESTORE_ACCOUNT);
}
