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
    private AccountViewModel _account; // TBR - need to be removed

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
    private async Task CloseAsync() => await Navigation.PopModalAsync();

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

    protected string GetControlErrorMessage(string nameOfControl)
    {
        var message = string.Empty;

        try
        {
            if (HasErrors)
            {
                Guard.IsNotNullOrWhiteSpace(nameOfControl);

                var errors = GetErrors(nameOfControl)?.Select(x => x.ErrorMessage).ToList();

                if (errors.Count > 0)
                {
                    message = errors[0];
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetControlErrorMessage({Value}) - Error: {Ex} ", nameOfControl, ex.Message);
        }

        return message;
    }
}
