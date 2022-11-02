namespace UC.Net.Node.MAUI.Pages;

public partial class TransactionsPage : CustomPage
{
    TransactionsViewModel Vm => BindingContext as TransactionsViewModel;

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
