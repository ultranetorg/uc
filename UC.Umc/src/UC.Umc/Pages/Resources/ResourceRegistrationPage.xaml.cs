using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Resources;
using UC.Umc.Views;

namespace UC.Umc.Pages.Resources;

public partial class ResourceRegistrationPage : CustomPage
{
	public ResourceRegistrationPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<ResourceRegistrationViewModel>();
	}

	public ResourceRegistrationPage(ResourceRegistrationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
