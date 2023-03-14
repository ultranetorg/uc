namespace UC.Umc.Pages;

public partial class ProductsListPage : CustomPage
{
    ProductsListViewModel Vm => BindingContext as ProductsListViewModel;

    public ProductsListPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ProductsListViewModel>();
    }

    public ProductsListPage(ProductsListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Vm.InitializeAsync();
    }
}
