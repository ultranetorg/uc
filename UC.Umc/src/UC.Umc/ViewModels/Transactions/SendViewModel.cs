using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Popups;

namespace UC.Umc.ViewModels.Transactions;

public partial class SendViewModel(ILogger<SendViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private AccountModel _source;

	[ObservableProperty]
	private AccountModel _recipient;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(FirstStep))]
	[NotifyPropertyChangedFor(nameof(SecondStep))]
	private int _position = 0;

	[ObservableProperty]
	[NotifyDataErrorInfo]
	[Required(ErrorMessage = "Required")]
	[Range(1.0, int.MaxValue, ErrorMessage = "Wrong Amount")] 
	[NotifyPropertyChangedFor(nameof(AmountError))]
	[NotifyPropertyChangedFor(nameof(Commission))]
	private string _amount;

	public decimal Commission 
	{
		get
		{
			decimal.TryParse((string?)Amount, out decimal commission);
			return commission > 0 ? commission / 100 : 0;
		}
	}

	public string AmountError => GetControlErrorMessage(nameof(SendViewModel.Amount));

	public bool FirstStep => Position == 0;

	public bool SecondStep => Position == 1;

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();

			Source = (AccountModel) query[QueryKeys.SOURCE_ACCOUNT];
			Recipient = (AccountModel) query[QueryKeys.RECIPIENT_ACCOUNT];
#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes Account: {Account}", Source ?? Recipient);
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
	private async Task CancelAsync()
	{
		try
		{
			if(Position == 1)
			{
				Position = 0;
			}
			else
			{
				await Navigation.PopAsync();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "CancelAsync Exception: {Ex}", ex.Message);
			ToastHelper.ShowErrorMessage(_logger);
		}
	}

	[RelayCommand]
	private async Task ConfirmAsync()
	{
		await Navigation.GoToAsync(Routes.COMPLETED_TRANSFERS);
	}
	
	[RelayCommand]
	private async Task SourceTappedAsync()
	{
		var popup = new SourceAccountPopup();
		await ShowPopup(popup);
		if (popup?.Vm?.Account != null)
		{
			Source = popup.Vm.Account;
		}
	}

	[RelayCommand]
	private async Task RecipientTappedAsync()
	{
		var popup = new RecipientAccountPopup();
		await ShowPopup(popup);
		if (popup?.Vm?.Account != null)
		{
			Recipient = popup.Vm.Account;
		}
	}
	
	[RelayCommand]
	private void Transfer()
	{
		if (Position == 0) 
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
	}
}
