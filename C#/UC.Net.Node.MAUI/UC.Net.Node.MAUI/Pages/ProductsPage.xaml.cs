namespace UC.Net.Node.MAUI.Pages;

public partial class ProductsPage : CustomPage
{
    ProductsViewModel Vm => BindingContext as ProductsViewModel;

    public ProductsPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<ProductsViewModel>();
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
