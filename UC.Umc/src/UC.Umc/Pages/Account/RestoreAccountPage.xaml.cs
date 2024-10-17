using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Accounts;
using UC.Umc.Views;

namespace UC.Umc.Pages.Account;

public partial class RestoreAccountPage : CustomPage
{
	public RestoreAccountPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<RestoreAccountViewModel>();
	}

	public RestoreAccountPage(RestoreAccountViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
