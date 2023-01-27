namespace UC.Umc.ViewModels;

public partial class ProductRegistrationViewModel : BaseViewModel
{
	[ObservableProperty]
	private string _name = string.Empty;

	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

	[ObservableProperty]
    private int _position;

    public ProductRegistrationViewModel(ILogger<ProductRegistrationViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		if (Position == 0)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully created!");
		}
	}

	[RelayCommand]
	protected void Prev()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
	}
}
