namespace UC.Net.Node.MAUI.Pages;

public partial class ProductsPage : CustomPage
{
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
}
