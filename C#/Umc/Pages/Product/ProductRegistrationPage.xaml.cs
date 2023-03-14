namespace UC.Umc.Pages;

public partial class ProductRegistrationPage : CustomPage
{
    public ProductRegistrationPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ProductRegistrationViewModel>();
    }

    public ProductRegistrationPage(ProductRegistrationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
