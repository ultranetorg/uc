using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Transactions;
using UC.Umc.Views;

namespace UC.Umc.Pages.Transactions;

public partial class ETHTransferPage : CustomPage
{
	public ETHTransferPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<ETHTransferViewModel>();
	}

	public ETHTransferPage(ETHTransferViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
