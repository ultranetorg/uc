namespace UC.Net.Node.MAUI.Pages;

public partial class ProductsBPage : CustomPage
{
    public ProductsBPage()
    {
        InitializeComponent();
        BindingContext = new ProductsBViewModel(ServiceHelper.GetService<ILogger<ProductsBViewModel>>());
    }
}
