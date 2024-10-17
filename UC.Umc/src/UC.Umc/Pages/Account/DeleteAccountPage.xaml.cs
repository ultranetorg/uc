using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Accounts;
using UC.Umc.Views;

namespace UC.Umc.Pages.Account;

public partial class DeleteAccountPage : CustomPage
{
	public DeleteAccountPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DeleteAccountViewModel>();
	}

	public DeleteAccountPage(DeleteAccountViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
