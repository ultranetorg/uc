namespace UC.Net.Node.MAUI.Pages;

public partial class ProductsBPage : CustomPage
{
    ProductsBViewModel Vm => BindingContext as ProductsBViewModel;

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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Vm.InitializeAsync();
    }
}
