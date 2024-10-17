using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Accounts;
using UC.Umc.Views;

namespace UC.Umc.Pages.Account;

public partial class ManageAccountsPage : CustomPage
{
	private ManageAccountsViewModel Vm => (BindingContext as ManageAccountsViewModel)!;

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
