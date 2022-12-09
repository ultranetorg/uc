namespace UC.Umc.ViewModels;

public abstract partial class BaseAccountViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<AccountColor> _colorsCollection = new();
	
	[ObservableProperty]
    private CustomCollection<Author> _authors = new();
	
	[ObservableProperty]
    private CustomCollection<Product> _products = new();

	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
    private int _position;

	protected BaseAccountViewModel(ILogger logger): base(logger)
	{
	}
	
	[RelayCommand]
    private void ColorTapped(AccountColor accountColor)
    {
        foreach (var item in ColorsCollection)
        {
            item.BorderColor = Colors.Transparent;
        }
        accountColor.BorderColor = Shell.Current.BackgroundColor;
        Account.Color = ColorHelper.CreateGradientColor(accountColor.Color);
    }

	[RelayCommand]
	protected async Task CloseAsync() => await Navigation.PopModalAsync();

	[RelayCommand]
	protected void Prev()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
	}

	[RelayCommand]
    protected void Next()
    {
        if (Position < 1)
		{
			Position += 1;
		}
    }
}
