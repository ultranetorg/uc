namespace UC.Net.Node.MAUI.Pages;

public partial class UnfinishTransferPage : CustomPage
{
    public UnfinishTransferPage()
    {
        InitializeComponent();
        BindingContext = new UnfinishTransferViewModel(ServiceHelper.GetService<ILogger<UnfinishTransferViewModel>>());
    }
}
public class UnfinishTransferViewModel : BaseViewModel
{
       
    public UnfinishTransferViewModel(ILogger<UnfinishTransferViewModel> logger) : base(logger)
    {
        Emissions.Add(new Emission { ETH = "100", Number=1,UNT="100" });
        Emissions.Add(new Emission { ETH = "1000", Number = 2, UNT = "1000" });
        Emissions.Add(new Emission { ETH = "10000", Number = 3, UNT = "10000" });
        Emissions.Add(new Emission { ETH = "100000", Number = 4, UNT = "10000" });
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
    CustomCollection<Emission> _Emissions = new CustomCollection<Emission>();
    public CustomCollection<Emission> Emissions
    {
        get { return _Emissions; }
        set { SetProperty(ref _Emissions, value); }
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
