using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;

namespace UC.Umc.ViewModels.Domains;

public partial class DomainTransferViewModel(ILogger<DomainTransferViewModel> logger) : BaseDomainViewModel(logger)
{
	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();

			Domain = (DomainModel) query[QueryKeys.AUTHOR];
#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes Author: {Domain}", Domain);
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
