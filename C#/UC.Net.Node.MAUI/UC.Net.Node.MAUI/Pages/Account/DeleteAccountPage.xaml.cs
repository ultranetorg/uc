namespace UC.Net.Node.MAUI.Pages;

public partial class DeleteAccountPage : CustomPage
{
    public DeleteAccountPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<DeleteAccountViewModel>();
		vm.Initialize(DefaultDataMock.Account1);
		BindingContext = vm;
    }

	public DeleteAccountPage(Account account)
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<DeleteAccountViewModel>();
		vm.Initialize(account);
        BindingContext = vm;
    }

	public DeleteAccountPage(Account account, DeleteAccountViewModel vm)
    {
        InitializeComponent();
		vm.Initialize(account);
        BindingContext = vm;
    }
}
