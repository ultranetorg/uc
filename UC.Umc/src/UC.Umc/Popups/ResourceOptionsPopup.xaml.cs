using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.Models;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class ResourceOptionsPopup : Popup
{
	private ResourceOptionsViewModel Vm => (BindingContext as ResourceOptionsViewModel)!;

	public ResourceOptionsPopup(ResourceModel resource)
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<ResourceOptionsViewModel>();
		Vm.Resource = resource;
		Vm.Popup = this;
	}
}
