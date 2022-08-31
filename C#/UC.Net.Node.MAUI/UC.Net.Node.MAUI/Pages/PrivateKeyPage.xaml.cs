namespace UC.Net.Node.MAUI.Pages;

public partial class PrivateKeyPage : CustomPage
{
    public PrivateKeyPage()
    {
        InitializeComponent();
		var vm = App.ServiceProvider.GetService<PrivateKeyViewModel>();
		vm.Wallet = new Wallet();
        BindingContext = vm;
    }

    public PrivateKeyPage(Wallet wallet)
    {
        InitializeComponent();
		var vm = App.ServiceProvider.GetService<PrivateKeyViewModel>();
		vm.Wallet = wallet;
        BindingContext = vm;
    }

    public PrivateKeyPage(Wallet wallet, PrivateKeyViewModel vm)
    {
        InitializeComponent();
		vm.Wallet = wallet;
        BindingContext = vm;
    }
}
