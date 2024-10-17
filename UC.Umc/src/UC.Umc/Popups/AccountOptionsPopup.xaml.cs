using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.Models;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class AccountOptionsPopup : Popup
{
	private AccountOptionsViewModel Vm => (BindingContext as AccountOptionsViewModel)!;

	public AccountOptionsPopup(AccountModel account)
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<AccountOptionsViewModel>();
		Vm.Account = account;
		Vm.Popup = this;
	}
}
