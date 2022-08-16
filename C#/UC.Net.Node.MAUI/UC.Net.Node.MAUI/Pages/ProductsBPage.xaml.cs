namespace UC.Net.Node.MAUI.Pages;

public partial class ProductsBPage : CustomPage
{
    public ProductsBPage(ProductsBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
