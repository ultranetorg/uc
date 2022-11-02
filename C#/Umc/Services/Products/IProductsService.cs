namespace UC.Umc.Services;

public interface IProductsService
{
    Task<Product> FindByAccountAddressAsync([NotNull, NotEmpty] string accountAddress);

    Task<int> GetCountAsync();

    Task<ObservableCollection<Product>> GetAllAsync();
}
