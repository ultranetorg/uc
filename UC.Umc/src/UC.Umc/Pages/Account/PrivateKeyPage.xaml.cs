using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Accounts;
using UC.Umc.Views;

namespace UC.Umc.Pages.Account;

public partial class PrivateKeyPage : CustomPage
{
	public PrivateKeyPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<PrivateKeyViewModel>();
	}

	public PrivateKeyPage(PrivateKeyViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
