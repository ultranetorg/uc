using System.Collections.ObjectModel;
using UC.Umc.Models;
using UC.Umc.Services;
using UC.Umc.Services.Resources;

namespace UC.Umc.ViewModels.Resources;

public partial class ResourceListViewModel(IResourcesService service, ILogger<ResourceListViewModel> logger)
	: BaseViewModel(logger)
{
	[ObservableProperty]
	private ResourceModel _selectedItem;
		
	[ObservableProperty]
	private ObservableCollection<ResourceModel> _resources = new();
	
	[ObservableProperty]
	private ObservableCollection<string> _resourcesFilter = new();

	[RelayCommand]
	private async Task OpenProductOptionsAsync(ResourceModel product)
	{
		// await AccountOptionsPopup.Show(author);
		await Task.Delay(10);
	}

	[RelayCommand]
	private async Task RegisterProductAsync(ResourceModel product)
	{
		// await AccountOptionsPopup.Show(author);
		await Task.Delay(10);
	}

	internal async Task InitializeAsync()
	{
		var products = await service.GetAllProductsAsync();
		Resources.AddRange(products);
		ResourcesFilter = DefaultDataMock.ProductsFilter;
	}
}
