namespace UC.Umc.Pages;

public partial class ManageAccountsPage : CustomPage
{
	ManageAccountsViewModel Vm => BindingContext as ManageAccountsViewModel;

    public ManageAccountsPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ManageAccountsViewModel>();
    }

    public ManageAccountsPage(ManageAccountsViewModel vm)
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
