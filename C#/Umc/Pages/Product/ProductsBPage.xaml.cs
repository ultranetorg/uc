namespace UC.Umc.Pages;

public partial class ProductsBPage : CustomPage
{
    ProductsBViewModel Vm => BindingContext as ProductsBViewModel;

    public ProductsBPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ProductsBViewModel>();
    }

    public ProductsBPage(ProductsBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Vm.InitializeAsync();
    }
}
