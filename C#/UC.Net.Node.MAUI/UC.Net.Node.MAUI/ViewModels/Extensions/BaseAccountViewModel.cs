namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseAccountViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<AccountColor> _colorsCollection = new();
	
	[ObservableProperty]
    private CustomCollection<Author> _authors = new();
	
	[ObservableProperty]
    private CustomCollection<Product> _products = new();

	[ObservableProperty]
    private Wallet _wallet;

	[ObservableProperty]
    private int _position;

	protected BaseAccountViewModel(ILogger logger): base(logger){}
	
	[RelayCommand]
    protected void ColorTapped(AccountColor accountColor)
    {
        foreach (var item in ColorsCollection)
        {
            item.BoderColor = Colors.Transparent;
            ColorsCollection.ReportItemChange(item);
        }
        accountColor.BoderColor = Shell.Current.BackgroundColor;
        ColorsCollection.ReportItemChange(accountColor);
        Wallet.AccountColor = accountColor.Color;
    }

	[RelayCommand]
    protected async void CloseAsync()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

	[RelayCommand]
    protected void Prev()
    {
        Position -= 1;
    }

	[RelayCommand]
    protected void Next()
    {
        if (Position == 1) return;
        Position += 1;
    }
}
