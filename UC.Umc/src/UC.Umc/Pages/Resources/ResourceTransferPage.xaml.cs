using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Resources;
using UC.Umc.Views;

namespace UC.Umc.Pages.Resources;

public partial class ResourceTransferPage : CustomPage
{
	public ResourceTransferPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<ResourceTransferViewModel>();
	}

	public ResourceTransferPage(ResourceTransferViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
