namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseAccountViewModel : BaseViewModel
{
    public Page Page { get; protected set; }

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
    private void ColorTapped(AccountColor accountColor)
    {
        foreach (var item in ColorsCollection)
        {
            item.BoderColor = Colors.Transparent;
            ColorsCollection.ReportItemChange(item);
        }
        accountColor.BoderColor = Page.BackgroundColor;
        ColorsCollection.ReportItemChange(accountColor);
        Wallet.AccountColor = accountColor.Color;
    }  

	[RelayCommand]
    private async void Close()
    {
        await Page.Navigation.PopAsync();
    }

	[RelayCommand]
    private void Prev()
    {
        Position -= 1;
    }

	[RelayCommand]
    private void Next()
    {
        if (Position == 1) return;
        Position += 1;
    }
}
