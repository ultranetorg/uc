namespace UC.Umc.Services;

public interface IProductsService
{
    Task<ProductViewModel> FindByAccountAddressAsync([NotNull, NotEmpty] string accountAddress);

    Task<int> GetCountAsync();

    Task<ObservableCollection<ProductViewModel>> GetAllAsync();
}
