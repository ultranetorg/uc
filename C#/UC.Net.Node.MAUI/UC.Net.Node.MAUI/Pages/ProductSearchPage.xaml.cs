namespace UC.Net.Node.MAUI.Pages;

public partial class ProductSearchPage : CustomPage
{
    public ProductSearchPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<ProductSearchViewModel>();
    }

    public ProductSearchPage(ProductSearchViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
