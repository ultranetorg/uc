using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.Models;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class TransactionPopup : Popup
{
	private TransactionDetailsViewModel Vm => (BindingContext as TransactionDetailsViewModel)!;

	public TransactionPopup(TransactionModel transaction)
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<TransactionDetailsViewModel>();
		Vm.Transaction = transaction;
		Vm.Account = transaction.Account;
		Vm.Popup = this;
	}
}
