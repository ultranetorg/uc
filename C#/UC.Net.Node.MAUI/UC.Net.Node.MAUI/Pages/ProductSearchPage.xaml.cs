namespace UC.Net.Node.MAUI.Pages;

public partial class ProductSearchPage : CustomPage
{
    ProductSearchViewModel Vm => BindingContext as ProductSearchViewModel;

    public ProductSearchPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ProductSearchViewModel>();
    }

    public ProductSearchPage(ProductSearchViewModel vm)
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
