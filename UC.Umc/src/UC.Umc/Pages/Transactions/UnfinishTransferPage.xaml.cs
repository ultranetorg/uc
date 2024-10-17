using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Transactions;
using UC.Umc.Views;

namespace UC.Umc.Pages.Transactions;

public partial class UnfinishTransferPage : CustomPage
{
	public UnfinishTransferPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<UnfinishTransferViewModel>();
	}
	
	public UnfinishTransferPage(UnfinishTransferViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
