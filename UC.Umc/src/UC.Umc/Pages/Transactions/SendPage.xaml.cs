using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Transactions;
using UC.Umc.Views;

namespace UC.Umc.Pages.Transactions;

public partial class SendPage : CustomPage
{
	public SendPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<SendViewModel>();
	}

	public SendPage(SendViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
