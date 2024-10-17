using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Pages.Resources;

namespace UC.Umc.ViewModels.Popups;

public partial class ResourceOptionsViewModel(ILogger<DomainOptionsViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private ResourceModel _resource;

	[RelayCommand]
	private async Task TransferProductAsync()
	{
		try
		{
			await Navigation.GoToAsync(nameof(ResourceTransferPage),
				new Dictionary<string, object>() { { QueryKeys.PRODUCT, Resource } });
			ClosePopup();
		}
		catch (Exception ex)
		{
			_logger.LogError("TransferProductAsync Error: {Message}", ex.Message);
		}
	}
}
