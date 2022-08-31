namespace UC.Net.Node.MAUI.Pages;

public partial class ProductsBPage : CustomPage
{
    public ProductsBPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<ProductsBViewModel>();
    }

    public ProductsBPage(ProductsBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
