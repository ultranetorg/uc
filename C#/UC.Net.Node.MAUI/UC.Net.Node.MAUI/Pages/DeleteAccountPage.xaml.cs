namespace UC.Net.Node.MAUI.Pages;

public partial class DeleteAccountPage : CustomPage
{
    public DeleteAccountPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<DeleteAccountViewModel>();
		vm.Initialize(new Wallet());
		BindingContext = vm;
    }

	public DeleteAccountPage(Wallet wallet)
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<DeleteAccountViewModel>();
		vm.Initialize(wallet);
        BindingContext = vm;
    }

	public DeleteAccountPage(Wallet wallet, DeleteAccountViewModel vm)
    {
        InitializeComponent();
		vm.Initialize(wallet);
        BindingContext = vm;
    }
}
