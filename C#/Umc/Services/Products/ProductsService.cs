namespace UC.Umc.Services;

public class ProductsService : IProductsService
{
    public Task<ProductViewModel> FindByAccountAddressAsync([NotEmpty, NotNull] string accountAddress)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ObservableCollection<ProductViewModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
}
