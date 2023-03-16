using UO.Mobile.UUC.Models;
using UO.Mobile.UUC.Pages.Products;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.Services.Products;
using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.ViewModels.Products;

internal class ProductsViewModel : BaseViewModel, IInitializableAsync
{
    // Properties and commands.
    public ICommand RegisterCommand => new Command(RegisterAsync);

    private ObservableCollection<Product> _products;

    private readonly IProductsService _productsService;
    private readonly INavigationService _navigationService;

    public ProductsViewModel(IProductsService productsService, INavigationService navigationService)
    {
        _productsService = productsService;
        _navigationService = navigationService;
    }

    // IInitializableAsync
    public bool IsInitialized { get; set; }

    public bool MultipleInitialization => true;

    public async Task InitializeAsync(object parameter)
    {
        IsInitialized = false;
        Products = await _productsService.GetAllAsync();
        IsInitialized = true;
    }

    // Bindings.
    private async void RegisterAsync()
    {
        await _navigationService.NavigateToAsync<RegisterPage>();
    }

    public ObservableCollection<Product> Products
    {
        get => _products;
        set
        {
            _products = value;
            OnPropertyChanged(nameof(Products));
        }
    }
}
