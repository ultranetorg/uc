namespace UC.Net.Node.MAUI.Services.Products;

public interface IProductsService
{
    Task<Product> FindByAccountAddressAsync([NotNull, NotEmpty] string accountAddress);

    Task<int> GetCountAsync();

    Task<ObservableCollection<Product>> GetAllAsync();
}
