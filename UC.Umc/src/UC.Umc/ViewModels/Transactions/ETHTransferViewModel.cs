using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Popups;
using UC.Umc.ViewModels.Accounts;

namespace UC.Umc.ViewModels.Transactions;

public partial class ETHTransferViewModel(ILogger<ETHTransferViewModel> logger) : BaseAccountViewModel(logger)
{
	[ObservableProperty]
	private bool _isPrivateKey = true;

	[ObservableProperty]
	private bool _isFilePath;

	[ObservableProperty]
	private bool _showFilePassword;

	[ObservableProperty]
	private string _privateKey;

	[ObservableProperty]
	private string _walletFilePath;

	[ObservableProperty]
	private string _walletFilePassword;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(UntAmount))]
	[NotifyPropertyChangedFor(nameof(EthCommission))]
	[NotifyPropertyChangedFor(nameof(UntCommission))]
	private decimal _ethAmount;

	public decimal UntAmount => EthAmount * 10;
	public decimal EthCommission => (EthAmount + 1) / 100;
	public decimal UntCommission => (EthAmount + 1) / 10;

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();

			Account = (AccountModel)query[QueryKeys.ACCOUNT];
			EthAmount = (decimal)query[QueryKeys.ETH_AMOUNT];
#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes Account: {Account}", Account);
			_logger.LogDebug("ApplyQueryAttributes EthAmount: {EthAmount}", Account);
#endif
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "ApplyQueryAttributes Exception: {Ex}", ex.Message);
			ToastHelper.ShowErrorMessage(_logger);
		}
		finally
		{
			FinishLoading();
		}
	}

	[RelayCommand]
	private void ChangeKeySource()
	{
		IsPrivateKey = !IsPrivateKey;
		IsFilePath = !IsFilePath;
	}

	[RelayCommand]
	private async Task OpenFilePickerAsync()
	{
		try
		{
			WalletFilePath = await CommonHelper.GetPathToWalletAsync();
			ShowFilePassword = !string.IsNullOrEmpty(WalletFilePath);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenFilePickerAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task ShowAccountsPopupAsync()
	{
		try
		{
			var popup = new SourceAccountPopup();
			await ShowPopup(popup);

			if (popup?.Vm?.Account != null)
			{
				Account = popup.Vm.Account;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("ShowAccountsPopupAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		try
		{
			if (Position == 0)
			{
				// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
				Position = 1;
				Position = 0;
				Position = 1;
			}
			else if (Position == 1)
			{
				Position = 2;
			}
			else
			{
				await Navigation.PopAsync();
				await ToastHelper.ShowMessageAsync("Successfully transfered!");
			}

			var account = Account;
		}
		catch (Exception ex)
		{
			_logger.LogError("NextWorkaroundAsync Error: {Message}", ex.Message);
		}
		
	}

	[RelayCommand]
	private async Task OpenOptionsPopupAsync()
	{
		try
		{
			await ShowPopup(new TransferOptionsPopup());
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenOptionsPopupAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task ConfirmAsync()
	{
		try
		{
			await Navigation.GoToAsync(Routes.COMPLETED_TRANSFERS, new Dictionary<string, object>()
			{
				{QueryKeys.ACCOUNT, Account},
				{QueryKeys.UNT_AMOUNT, UntAmount }
			});
		}
		catch (Exception ex)
		{
			_logger.LogError("ConfirmAsync Error: {Message}", ex.Message);
		}
	}
}
