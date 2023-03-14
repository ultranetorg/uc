namespace UC.Umc.Pages;

public partial class ProductTransferPage : CustomPage
{
	public ProductTransferPage()
	{
		InitializeComponent();
        BindingContext = Ioc.Default.GetService<ProductTransferViewModel>();
    }

    public ProductTransferPage(ProductTransferViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}