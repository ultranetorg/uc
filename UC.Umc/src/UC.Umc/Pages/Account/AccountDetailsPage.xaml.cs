using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.Models;
using UC.Umc.ViewModels.Accounts;
using UC.Umc.Views;

namespace UC.Umc.Pages.Account;

public partial class AccountDetailsPage : CustomPage
{
	public AccountDetailsPage()
	{
		InitializeComponent();
		var vm = Ioc.Default.GetService<AccountDetailsViewModel>();
		BindingContext = vm;
	}

	public AccountDetailsPage(AccountModel account)
	{
		InitializeComponent();
		var vm = Ioc.Default.GetService<AccountDetailsViewModel>();
		vm.Account = account;
		BindingContext = vm;
	}
}
