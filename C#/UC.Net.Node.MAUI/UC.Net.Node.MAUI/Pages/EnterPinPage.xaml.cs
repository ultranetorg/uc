namespace UC.Net.Node.MAUI.Pages;

public partial class EnterPinPage : CustomPage
{
    public EnterPinPage()
    {
        InitializeComponent();
        BindingContext = new EnterPinViewModel(ServiceHelper.GetService<ILogger<EnterPinViewModel>>());
    }
}
public class EnterPinViewModel : BaseViewModel
{
       
    public EnterPinViewModel(ILogger<EnterPinViewModel> logger) : base(logger)
    {
    }

    public Command DeleteCommand
    {
        get => new Command(Delete);
    }

    private async void Delete()
    {
        await DeleteAccountPopup.Show(Wallet);
    }

    public Command TransactionsCommand
    {
        get => new Command(Transactions);
    }

    private async void Transactions()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
        
    Wallet _Wallet = new Wallet
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };
    public Wallet Wallet
    {
        get { return _Wallet; }
        set { SetProperty(ref _Wallet, value); }
    }
}
