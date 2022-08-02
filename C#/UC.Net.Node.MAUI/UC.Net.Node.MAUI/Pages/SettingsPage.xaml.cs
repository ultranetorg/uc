namespace UC.Net.Node.MAUI.Pages;

public partial class SettingsPage : CustomPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel(ServiceHelper.GetService<ILogger<SettingsViewModel>>());
    }
}
public class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(ILogger<SettingsViewModel> logger) : base(logger)
    {
        Months.Add("April");
        Months.Add("May");
        Months.Add("June");
        Months.Add("July");
        Months.Add("Augest");
        Months.Add("Spetemper");
        Months.Add("November");
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
    CustomCollection<string> _Months = new CustomCollection<string>();
    public CustomCollection<string> Months
    {
        get { return _Months; }
        set { SetProperty(ref _Months, value); }
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
