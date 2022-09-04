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
    private void ColorTapped(AccountColor accountColor)
    {
        foreach (var item in ColorsCollection)
        {
            item.BoderColor = Colors.Transparent;
        }
        accountColor.BoderColor = Shell.Current.BackgroundColor;
        Wallet.AccountColor = accountColor.Color;
    }

	[RelayCommand]
    private async void CloseAsync()
    {
        await Shell.Current.Navigation.PopModalAsync();
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
