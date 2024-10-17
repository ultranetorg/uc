using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Popups;
using UC.Umc.Services;
using UC.Umc.Services.Resources;

namespace UC.Umc.ViewModels.Resources;

public partial class ResourceViewModel(IResourcesService service, ILogger<ResourceViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private ResourceViewModel _selectedItem;

	[ObservableProperty]
	private ObservableCollection<ResourceModel> _resources = new();

	[ObservableProperty]
	private ObservableCollection<string> _productsFilter = new();

	[RelayCommand]
	private async Task OpenProductOptionsAsync(ResourceModel resource)
	{
		try
		{
			await ShowPopup(new ResourceOptionsPopup(resource));
		}
		catch(ArgumentException ex)
		{
			_logger.LogError("OpenOptionsAsync: Product cannot be null, Error: {Message}", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenOptionsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task RegisterProductAsync(ResourceViewModel product) =>
		await Navigation.GoToAsync(nameof(ResourceViewModel),
			new Dictionary<string, object>() { { QueryKeys.PRODUCT, product } });

	internal async Task InitializeAsync()
	{
		ObservableCollection<ResourceModel> products = await service.GetAllProductsAsync();
		Resources.AddRange(products);
		ProductsFilter = DefaultDataMock.ProductsFilter;
	}
}
