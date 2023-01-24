using System.Globalization;

namespace UC.Umc.Services;

public class ProductsMockService : IProductsService
{
    private readonly IServicesMockData _data;

    public ProductsMockService(IServicesMockData data)
    {
        _data = data;
    }

	public Task<ObservableCollection<ProductViewModel>> GetAllProductsAsync() =>
		Task.FromResult(new ObservableCollection<ProductViewModel>(_data.Products.ToList()));

	public Task<ObservableCollection<ProductViewModel>> GetAuthorProductsAsync(string authorName) =>
		Task.FromResult(new ObservableCollection<ProductViewModel>(_data.Products.Where(x => x.Author.Name == authorName)));

	public Task<ObservableCollection<ProductViewModel>> SortProductsAsync(string sortBy)
	{
		var products = _data.Products.AsQueryable().OrderBy(sortBy);
		return Task.FromResult(new ObservableCollection<ProductViewModel>(products.ToList()));
	}

    public Task<ObservableCollection<ProductViewModel>> SearchProductsAsync(string search)
    {
		var items = _data.Products.Where(x => x.Name.Contains(search)).ToList();
        var result = new ObservableCollection<ProductViewModel>(items);
        return Task.FromResult(result);
    }
}
