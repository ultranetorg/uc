using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;

namespace UC.Umc.ViewModels.Accounts;

public partial class PrivateKeyViewModel(ILogger<PrivateKeyViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private ObservableCollection<DomainModel> _domains = new();

	[ObservableProperty]
	private ObservableCollection<ResourceModel> _resources = new();
	
	[ObservableProperty]
	private AccountModel _account;

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();

			Account = (AccountModel)query[QueryKeys.ACCOUNT];
#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes Account: {Account}", Account);
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
	private async Task CopyAsync()
	{
		try
		{
			await Clipboard.SetTextAsync(Account.Address);
			await ToastHelper.ShowMessageAsync("Copied to clipboard");
#if DEBUG
			_logger.LogDebug("CopyAsync Address: {Address}", Account.Address);
#endif
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "CopyAsync Exception: {Ex}", ex.Message);
			ToastHelper.ShowErrorMessage(_logger);
		}
	}

	[RelayCommand]
	private async Task CancelAsync()
	{
		try
		{
			await Navigation.PopAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "CancelAsync Exception: {Ex}", ex.Message);
			ToastHelper.ShowErrorMessage(_logger);
		}
	}
}
