namespace UC.Net.Node.MAUI.Pages;

public partial class PrivateKeyPage : CustomPage
{
    public PrivateKeyPage()
    {
        InitializeComponent();
		var vm = Ioc.Default.GetService<PrivateKeyViewModel>();
		vm.Account = DefaultDataMock.Account1;
        BindingContext = vm;
    }

    public PrivateKeyPage(Account account)
    {
        InitializeComponent();
		var vm = Ioc.Default.GetService<PrivateKeyViewModel>();
		vm.Account = account;
        BindingContext = vm;
    }

    public PrivateKeyPage(Account account, PrivateKeyViewModel vm)
    {
        InitializeComponent();
		vm.Account = account;
        BindingContext = vm;
    }
}
