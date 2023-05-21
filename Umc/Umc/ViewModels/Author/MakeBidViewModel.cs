using System.ComponentModel.DataAnnotations;

namespace UC.Umc.ViewModels;

public partial class MakeBidViewModel : BaseAuthorViewModel
{
	[ObservableProperty]
	[NotifyDataErrorInfo]
	[Required(ErrorMessage = "Required")]
	[Range(1.0, int.MaxValue, ErrorMessage = "Wrong Amount")]
	[NotifyPropertyChangedFor(nameof(AmountError))]
	private string _amount;

	public string AmountError => GetControlErrorMessage(nameof(Amount));

	public MakeBidViewModel(INotificationsService notificationService, ILogger<MakeBidViewModel> logger) : base(notificationService, logger)
    {
	}

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();

			Author = (AuthorViewModel)query[QueryKeys.AUTHOR];
#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes Author: {Author}", Author);
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
	private async Task NextWorkaroundNewAsync()
	{
		var isValid = Account != null && !string.IsNullOrEmpty(Amount);
		if (Position == 0 && isValid)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else if (Position == 1)
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Success!");
		}
	}

	[RelayCommand]
	protected async Task CancelAsync()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
		else
		{
			await Navigation.PopAsync();
		}
	}
}
