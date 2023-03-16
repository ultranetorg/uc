using UO.Mobile.UUC.Models;

namespace UO.Mobile.UUC.Services.Products;

public interface IProductsService
{
    Task<Product> FindByAccountAddressAsync([NotNull, NotEmpty] string accountAddress);

    Task<int> GetCountAsync();

    Task<ObservableCollection<Product>> GetAllAsync();
}
