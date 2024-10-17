using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Domains;
using UC.Umc.Views;

namespace UC.Umc.Pages.Domains;

public partial class MakeBidPage : CustomPage
{
	public MakeBidPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<MakeBidViewModel>();
	}

	public MakeBidPage(MakeBidViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
