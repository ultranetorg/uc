namespace UC.Net.Node.MAUI.Pages;

public partial class ProductsPage : CustomPage
{
    public ProductsPage()
    {
        InitializeComponent();
        BindingContext = new ProductsViewModel(ServiceHelper.GetService<ILogger<ProductsViewModel>>());
    }
}
