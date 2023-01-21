using System.ComponentModel.DataAnnotations;

namespace UC.Umc.ViewModels;

public partial class MakeBidViewModel : BaseAuthorViewModel
{
	[ObservableProperty]
	[NotifyDataErrorInfo]
	[Required(ErrorMessage = "Required")]
	[Range(1.0, int.MaxValue, ErrorMessage = "Wrong Amount")]
	[NotifyPropertyChangedFor(nameof(AmountError))]
	[NotifyPropertyChangedFor(nameof(Comission))]
	private string _amount;

	public decimal Comission
	{
		get
		{
			decimal.TryParse(Amount, out decimal comission);
			return comission > 0 ? comission / 100 : 0;
		}
	}

	public string AmountError => GetControlErrorMessage(nameof(Amount));

	public MakeBidViewModel(ILogger<MakeBidViewModel> logger) : base(logger)
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
}
