namespace UC.Net.Node.MAUI.Pages;

public partial class ProductSearchPage : CustomPage
{
    public ProductSearchPage(ProductSearchViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
