using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Models.Common;
using UC.Umc.Pages.Account;
using UC.Umc.Pages.Transactions;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Accounts;

public partial class AccountDetailsViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
	[NotifyDataErrorInfo]
	[Required(ErrorMessage = "Required")]
	[NotifyPropertyChangedFor(nameof(AccountNameError))]
	private string _accountName;

	public string AccountNameError => GetControlErrorMessage(nameof(AccountName));

	[ObservableProperty]
	public GradientBrush _background;

	public AccountDetailsViewModel(IServicesMockData service, ILogger<AccountDetailsViewModel> logger) : base(logger)
	{
		_service = service;
		LoadData();
	}

	// !TO DO: set background in model only in VM after submitting

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();

			Account = (AccountModel) query[QueryKeys.ACCOUNT];
			AccountName = Account.Name;
			Background = Account.Color;
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
	private async Task SendAsync() =>
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, Account },
			{ QueryKeys.RECIPIENT_ACCOUNT, null }
		});

	[RelayCommand]
	private async Task ReceiveAsync() =>
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, null },
			{ QueryKeys.RECIPIENT_ACCOUNT, Account }
		});

	[RelayCommand]
	private async Task ShowPrivateKeyAsync() =>
		await Navigation.GoToAsync(nameof(PrivateKeyPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.ACCOUNT, Account }
		});

	[RelayCommand]
	private async Task DeleteAsync() =>
		await Navigation.GoToAsync(nameof(DeleteAccountPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.ACCOUNT, Account }
		});

	[RelayCommand]
	private void SetAccountColor(AccountColor accountColor) =>
		Background = accountColor != null 
			? ColorHelper.CreateGradientColor(accountColor.Color)
			: ColorHelper.CreateRandomGradientColor();

	[RelayCommand]
	private async Task BackupAsync()
	{
		// TODO
		await Task.Delay(1);
	}

	[RelayCommand]
	private async Task HideFromDashboardAsync()
	{
		// TODO
		await Task.Delay(1);
	}

	private void LoadData()
	{
		Domains.Clear();
		Resources.Clear();
		ColorsCollection.Clear();

		Domains.AddRange(_service.Domains);
		Resources.AddRange(_service.Resources);
		ColorsCollection.AddRange(_service.AccountColors);
	}
}
