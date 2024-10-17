using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Accounts;
using UC.Umc.Views;

namespace UC.Umc.Pages.Account;

public partial class CreateAccountPage : CustomPage
{
	private CreateAccountPageViewModel Vm => (BindingContext as CreateAccountPageViewModel)!;

	public CreateAccountPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<CreateAccountPageViewModel>();
	}

	public CreateAccountPage(CreateAccountPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Vm.InitializeAsync();
	}
}
