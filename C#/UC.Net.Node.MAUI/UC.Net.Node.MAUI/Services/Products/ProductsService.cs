namespace UC.Net.Node.MAUI.Services;

public class ProductsService : IProductsService
{
    public Task<Product> FindByAccountAddressAsync([NotEmpty, NotNull] string accountAddress)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ObservableCollection<Product>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
}
