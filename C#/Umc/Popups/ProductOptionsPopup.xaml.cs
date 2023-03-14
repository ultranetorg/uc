namespace UC.Umc.Popups;

public partial class ProductOptionsPopup : Popup
{
	ProductOptionsViewModel Vm => BindingContext as ProductOptionsViewModel;

    public ProductOptionsPopup(ProductViewModel product)
    {
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<ProductOptionsViewModel>();
		Vm.Product = product;
		Vm.Popup = this;
    }
}