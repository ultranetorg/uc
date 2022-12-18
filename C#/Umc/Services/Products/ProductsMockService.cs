namespace UC.Umc.Services;

public class ProductsMockService : IProductsService
{
    private readonly IServicesMockData _data;

    public ProductsMockService(IServicesMockData data)
    {
        _data = data;
    }

    public Task<ProductViewModel> FindByAccountAddressAsync([NotEmpty, NotNull] string accountAddress)
    {
        Guard.IsNotNullOrEmpty(accountAddress, nameof(accountAddress));

        ProductViewModel result = _data.Products.FirstOrDefault(x => x.Author.Account.Address == accountAddress);
        return Task.FromResult(result);
    }

    public Task<int> GetCountAsync()
    {
        int result = _data.Products.Count;
        return Task.FromResult(result);
    }

    public Task<ObservableCollection<ProductViewModel>> GetAllAsync()
    {
        ObservableCollection<ProductViewModel> result = new(_data.Products);
        return Task.FromResult(result);
    }
}
