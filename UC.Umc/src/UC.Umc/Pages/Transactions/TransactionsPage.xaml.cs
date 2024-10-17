using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Transactions;
using UC.Umc.Views;

namespace UC.Umc.Pages.Transactions;

public partial class TransactionsPage : CustomPage
{
	private TransactionsViewModel Vm => (BindingContext as TransactionsViewModel)!;

	public TransactionsPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<TransactionsViewModel>();
	}

	public TransactionsPage(TransactionsViewModel vm)
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
