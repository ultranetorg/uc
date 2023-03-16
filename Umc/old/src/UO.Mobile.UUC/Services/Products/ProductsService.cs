using UO.Mobile.UUC.Models;

namespace UO.Mobile.UUC.Services.Products;

public class ProductsService : IProductsService
{
    public Task<Product> FindByAccountAddressAsync([NotEmpty, NotNull] string accountAddress)
    {
        throw new System.NotImplementedException();
    }

    public Task<int> GetCountAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<ObservableCollection<Product>> GetAllAsync()
    {
        throw new System.NotImplementedException();
    }
}
