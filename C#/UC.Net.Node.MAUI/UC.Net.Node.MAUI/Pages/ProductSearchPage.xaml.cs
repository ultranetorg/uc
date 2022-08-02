namespace UC.Net.Node.MAUI.Pages;

public partial class ProductSearchPage : CustomPage
{
    public ProductSearchPage()
    {
        InitializeComponent();
        BindingContext = new ProductSearchViewModel(ServiceHelper.GetService<ILogger<ProductSearchViewModel>>());
    }
}
