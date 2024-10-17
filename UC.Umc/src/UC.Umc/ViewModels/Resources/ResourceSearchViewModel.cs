using System.Collections.ObjectModel;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Services;
using UC.Umc.Services.Resources;

namespace UC.Umc.ViewModels.Resources;

public partial class ResourceSearchViewModel(IResourcesService service, ILogger<ResourceSearchViewModel> logger)
	: BaseViewModel(logger)
{
	[ObservableProperty]
	private ObservableCollection<ResourceModel> _resources = new();
	
	[ObservableProperty]
	private ObservableCollection<string> _productsFilter = new();

	[ObservableProperty]
	private string _filter;

	[RelayCommand]
	public async Task SearchProductsAsync()
	{
		try
		{
			Guard.IsNotNull(Filter);

			InitializeLoading();

			// Search products
			var products = await service.SearchProductsAsync(Filter);

			Resources.Clear();
			Resources.AddRange(products);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchProductsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task SortProductsAsync(string sortBy)
	{
		try
		{
			Guard.IsNotNullOrEmpty(sortBy);

			InitializeLoading();

			// Sort products
			var products = await service.GetAllProductsAsync();
			var ordered = products.AsQueryable().OrderBy(x => sortBy == "Author"
				? x.Owner : sortBy == "Version"
				? x.Version : sortBy == "Name"
				? x.Name : null);

			Resources.Clear();
			Resources.AddRange(ordered);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SortProductsAsync Error: {Message}", ex.Message);
		}
	}

	internal async Task InitializeAsync()
	{
		try
		{
			InitializeLoading();

			var products = await service.GetAllProductsAsync();
			Resources.Clear();
			Resources.AddRange(products);
			ProductsFilter = DefaultDataMock.ProductsFilter;

			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("InitializeAsync Error: {Message}", ex.Message);
		}
	}
}
