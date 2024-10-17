using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.Models;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class DeleteAccountPopup : Popup
{
	private DeleteAccountPopupViewModel Vm => (BindingContext as DeleteAccountPopupViewModel)!;

	public DeleteAccountPopup(AccountModel account)
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DeleteAccountPopupViewModel>();
		Vm.Account = account;
		Vm.Popup = this;
	}
}
