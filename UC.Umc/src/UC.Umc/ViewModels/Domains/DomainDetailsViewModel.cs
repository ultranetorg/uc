using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Domains;

public partial class DomainDetailsViewModel : BaseDomainViewModel
{
	private readonly IServicesMockData _service;
	
	[ObservableProperty]
	private ObservableCollection<ResourceModel> _resources = new();
	
	[ObservableProperty]
	private ObservableCollection<Bid> _bidsHistory = new();

	public DomainDetailsViewModel(IServicesMockData service, ILogger<DomainDetailsViewModel> logger) : base(logger)
	{
		_service = service;
		LoadData();
	}

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();

			Domain = (DomainModel) query[QueryKeys.AUTHOR];
#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes Author: {Author}", Domain);
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

	private void LoadData()
	{
		Resources.Clear();
		BidsHistory.Clear();

		Resources.AddRange(_service.Resources);
		BidsHistory.AddRange(_service.BidsHistory);
		
		// TODO: add form object, the account is coming from api
	}

	[RelayCommand]
	private async Task HideFromDashboardAsync()
	{
		// TODO
		await Task.Delay(1);
	}
}
