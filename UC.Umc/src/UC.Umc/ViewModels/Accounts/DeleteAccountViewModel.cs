using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Popups;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Accounts;

public partial class DeleteAccountViewModel : BaseAccountViewModel
{
	// will be splitted into 3 services
	private readonly IServicesMockData _service;

	[ObservableProperty]
	private ObservableCollection<DomainModel> _domains = new();

	[ObservableProperty]
	private ObservableCollection<ResourceModel> _resources = new();

	public DeleteAccountViewModel(IServicesMockData service, ILogger<DeleteAccountViewModel> logger) : base(logger)
	{
		_service = service;
		LoadData();
	}

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
	private async Task DeleteAsync()
	{
		try
		{
			await ShowPopup(new DeleteAccountPopup(Account));
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully deleted!");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "DeleteAsync Exception: {Ex}", ex.Message);
			await ToastHelper.ShowDefaultErrorMessageAsync();
		}
	}

	private void LoadData()
	{
		Domains.Clear();
		Resources.Clear();

		Domains.AddRange(_service.Domains);
		Resources.AddRange(_service.Resources);
	}
}
