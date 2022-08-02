namespace UC.Net.Node.MAUI.Pages;

public partial class WhatsNewPage : CustomPage
{
    public WhatsNewPage()
    {
        InitializeComponent();
        BindingContext = new WhatsNewViewModel(ServiceHelper.GetService<ILogger<WhatsNewViewModel>>());
    }
}
public class WhatsNewViewModel : BaseViewModel
{
       
    public WhatsNewViewModel(ILogger<WhatsNewViewModel> logger) : base(logger)
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
