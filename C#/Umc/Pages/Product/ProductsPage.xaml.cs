namespace UC.Umc.Pages;

public partial class ProductsPage : CustomPage
{
    ProductsViewModel Vm => BindingContext as ProductsViewModel;

    public ProductsPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ProductsViewModel>();
    }

    public ProductsPage(ProductsViewModel vm)
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
