using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.Models;
using UC.Umc.ViewModels.Domains;
using UC.Umc.Views;

namespace UC.Umc.Pages.Domains;

public partial class DomainDetailsPage : CustomPage
{
	public DomainDetailsPage(DomainModel author, DomainDetailsViewModel vm)
	{
		InitializeComponent();
		vm.Domain = author;
		BindingContext = vm;
	}

	public DomainDetailsPage(DomainModel author)
	{
		InitializeComponent();
		var vm = Ioc.Default.GetService<DomainDetailsViewModel>();
		vm.Domain = author;
		BindingContext = vm;
	}

	public DomainDetailsPage()
	{
		InitializeComponent();
		var vm = Ioc.Default.GetService<DomainDetailsViewModel>();
		vm.Domain = new DomainModel();
		BindingContext = vm;
	}
}
