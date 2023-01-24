namespace UC.Umc.Services;

public interface IProductsService
{
    Task<ObservableCollection<ProductViewModel>> GetAllProductsAsync();

    Task<ObservableCollection<ProductViewModel>> GetAuthorProductsAsync(string authorName);

    Task<ObservableCollection<ProductViewModel>> SortProductsAsync(string sortBy);

    Task<ObservableCollection<ProductViewModel>> SearchProductsAsync(string search);
}
