namespace UC.Umc.Services;

public interface IProductsService
{
    CustomCollection<ProductViewModel> GetAccountProducts(string account);

    Task<ObservableCollection<ProductViewModel>> GetAllProductsAsync();

    Task<ObservableCollection<ProductViewModel>> SearchProductsAsync(string search);
}
