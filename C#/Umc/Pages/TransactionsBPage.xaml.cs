namespace UC.Net.Node.MAUI.Pages;

public partial class TransactionsBPage : CustomPage
{
    TransactionsBViewModel Vm => BindingContext as TransactionsBViewModel;

    public TransactionsBPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<TransactionsBViewModel>();
    }

    public TransactionsBPage(TransactionsBViewModel vm)
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
