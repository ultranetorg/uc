namespace UC.Net.Node.MAUI.Pages;

public partial class HelpBPage : CustomPage
{
    public HelpBPage()
    {
        InitializeComponent();
        BindingContext = new HelpBViewModel(ServiceHelper.GetService<ILogger<HelpBViewModel>>());
    }
}
public class HelpBViewModel : BaseViewModel
{
    public HelpBViewModel(ILogger<HelpBViewModel> logger) : base(logger)
    {
    }

    public Command CancelCommand
    {
        get => new Command(Cancel);
    }

    private async void Cancel()
    {
        await Shell.Current.Navigation.PopAsync();
    }

    public Command TransactionsCommand
    {
        get => new Command(Transactions);
    }

    private async void Transactions()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }  
}
