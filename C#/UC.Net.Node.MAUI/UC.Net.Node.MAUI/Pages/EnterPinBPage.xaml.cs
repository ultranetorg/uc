namespace UC.Net.Node.MAUI.Pages;

public partial class EnterPinBPage : CustomPage
{
    public EnterPinBPage()
    {
        InitializeComponent();
        BindingContext = new EnterPinBViewModel(ServiceHelper.GetService<ILogger<EnterPinBViewModel>>());
    }
}
public partial class EnterPinBViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _wallet = new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };

    public EnterPinBViewModel(ILogger<EnterPinBViewModel> logger) : base(logger)
    {
    }

    [RelayCommand]
    private async void Delete()
    {
        await DeleteAccountPopup.Show(Wallet);
    }
	
    [RelayCommand]
    private async void Transactions()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}
