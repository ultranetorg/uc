using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Popups;

namespace UC.Umc.ViewModels.Resources;

public partial class ResourceTransferViewModel : BaseViewModel
{
	[ObservableProperty]
	private DomainModel _domain;

	[ObservableProperty]
	private ResourceModel _resource;

	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

	[ObservableProperty]
	private int _position;

	public ResourceTransferViewModel(ILogger<ResourceTransferViewModel> logger) : base(logger)
	{
	}

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();

			Resource = (ResourceModel) query[QueryKeys.PRODUCT];
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

	[RelayCommand]
	private async Task SelectAuthorAsync()
	{
		try
		{
			var popup = new SelectDomainPopup();
			await ShowPopup(popup);
			if (popup.Vm?.SelectedAuthor != null)
			{
				Domain = popup.Vm.SelectedAuthor;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "SelectAuthorAsync Exception: {Ex}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		if (Position == 0)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully transfered!");
		}
	}

	[RelayCommand]
	protected void Prev()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
	}
}
