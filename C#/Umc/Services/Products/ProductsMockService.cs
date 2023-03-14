namespace UC.Umc.Services;

public class ProductsMockService : IProductsService
{
    private readonly IServicesMockData _service;

    public ProductsMockService(IServicesMockData data)
    {
        _service = data;
    }

	public Task<ObservableCollection<ProductViewModel>> GetAllProductsAsync() =>
		Task.FromResult(new ObservableCollection<ProductViewModel>(_service.Products.ToList()));

	public Task<ObservableCollection<ProductViewModel>> GetAuthorProductsAsync(string authorName) =>
		Task.FromResult(new ObservableCollection<ProductViewModel>(_service.Products.Where(x => x.Author.Name == authorName)));

    public Task<ObservableCollection<ProductViewModel>> SearchProductsAsync(string search)
    {
		var items = _service.Products.Where(x => x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToList();
        var result = new ObservableCollection<ProductViewModel>(items);
        return Task.FromResult(result);
    }
}
