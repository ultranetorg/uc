using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Popups;
using UC.Umc.Services;
using UC.Umc.Services.Transactions;

namespace UC.Umc.ViewModels.Transactions;

public partial class TransactionsViewModel : BaseViewModel
{
	private readonly ITransactionsService _service;

	[ObservableProperty]
	private ObservableCollection<TransactionModel> _transactions = new();

	[ObservableProperty]
	private string _filter;

	[ObservableProperty]
	private AccountModel _account;

	public TransactionsViewModel(ITransactionsService service, ILogger<TransactionsViewModel> logger) : base(logger)
	{
		_service = service;
	}

	internal async Task InitializeAsync()
	{
		try
		{
			Account = DefaultDataMock.AllAccountOption;
			Transactions = await _service.ListTransactionsAsync(null, null, SizeConstants.SizePerPageMed);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "InitializeAsync Exception: {Ex}", ex.Message);
		}
	}
	
	[RelayCommand]
	public async Task SearchTransactionsAsync()
	{
		try
		{
			Guard.IsNotNull<string>(Filter);

			InitializeLoading();

			// Search transactions
			var transactions = await _service.ListTransactionsAsync(Account?.Address, Filter, SizeConstants.SizePerPageMed);

			Transactions.Clear();
			Transactions.AddRange(transactions);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchTransactionsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task SelectAccountAsync()
	{
		try
		{
			var popup = new SourceAccountPopup(true);
			await ShowPopup(popup);
			if (popup.Vm?.Account != null)
			{
				Account = popup.Vm.Account;
				Transactions = await _service.ListTransactionsAsync(Account.Address, Filter, SizeConstants.SizePerPageMed);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "SelectAccountAsync Exception: {Ex}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task ResetAccountAsync()
	{
		try
		{
			await InitializeAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "ResetAccount Exception: {Ex}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task OpenDetailsAsync(TransactionModel transaction)
	{
		try
		{
			Guard.IsNotNull(transaction);

			await ShowPopup(new TransactionPopup(transaction));
		}
		catch(ArgumentException ex)
		{
			_logger.LogError("OpenDetailsAsync: Transaction cannot be null, Error: {Message}", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenDetailsAsync Error: {Message}", ex.Message);
		}
	}
}
