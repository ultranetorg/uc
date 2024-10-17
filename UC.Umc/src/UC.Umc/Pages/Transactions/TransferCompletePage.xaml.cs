using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Transactions;
using UC.Umc.Views;

namespace UC.Umc.Pages.Transactions;

public partial class TransferCompletePage : CustomPage
{
	public TransferCompletePage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<TransferCompleteViewModel>();
	}

	public TransferCompletePage(TransferCompleteViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
