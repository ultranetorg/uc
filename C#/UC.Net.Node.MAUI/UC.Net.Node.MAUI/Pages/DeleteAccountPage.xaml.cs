namespace UC.Net.Node.MAUI.Pages;

public partial class DeleteAccountPage : CustomPage
{
    public DeleteAccountPage(Wallet wallet)
    {
        InitializeComponent();
        var vm = App.ServiceProvider.GetService<DeleteAccountViewModel>();
		vm.FillFakeData(wallet);
		BindingContext = vm;
    }

	public DeleteAccountPage(Wallet wallet, DeleteAccountViewModel vm)
    {
        InitializeComponent();
		vm.FillFakeData(wallet);
        BindingContext = vm;
    }
}
