namespace UC.Umc.Services;

public class ProductsService : IProductsService
{
	public Task<ObservableCollection<ProductViewModel>> GetAllProductsAsync()
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<ProductViewModel>> GetAuthorProductsAsync(string authorName)
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<ProductViewModel>> SearchProductsAsync(string search)
	{
		throw new NotImplementedException();
	}
}
