namespace UC.Net.Node.MAUI.Pages;

public partial class ProductsPage : CustomPage
{
    public ProductsPage(ProductsViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<ProductsViewModel>();
    }
}
