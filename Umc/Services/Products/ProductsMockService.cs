namespace UC.Umc.Services;

public class ProductsMockService : IProductsService
{
    private readonly IServicesMockData _service;

    public ProductsMockService(IServicesMockData data)
    {
        _service = data;
    }

    public CustomCollection<ProductViewModel> GetAccountProducts(string account)
	{
		var products = _service.Products.Where(x => x.Author.Account.Address == account).ToList();
		return new CustomCollection<ProductViewModel>(products);
	}

	public async Task<ObservableCollection<ProductViewModel>> GetAllProductsAsync() =>
		await Task.FromResult(new ObservableCollection<ProductViewModel>(_service.Products.ToList()));

    public async Task<ObservableCollection<ProductViewModel>> SearchProductsAsync(string search)
    {
		var items = _service.Products.Where(x => x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToList();
        var result = new ObservableCollection<ProductViewModel>(items);
        return await Task.FromResult(result);
    }
}
