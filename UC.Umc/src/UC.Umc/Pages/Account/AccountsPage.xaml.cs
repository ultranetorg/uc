using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Accounts;
using UC.Umc.Views;

namespace UC.Umc.Pages.Account;

public partial class AccountsPage : CustomPage
{
	private AccountsViewModel Vm => (BindingContext as AccountsViewModel)!;

	public AccountsPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<AccountsViewModel>();
	}

	public AccountsPage(AccountsViewModel vm)
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
