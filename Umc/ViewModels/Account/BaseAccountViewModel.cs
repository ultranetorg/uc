namespace UC.Umc.ViewModels;

public abstract partial class BaseAccountViewModel : BasePageViewModel
{
	[ObservableProperty]
    private CustomCollection<AccountColor> _colorsCollection = new();
	
	[ObservableProperty]
    private CustomCollection<AuthorViewModel> _authors = new();
	
	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();

	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
    private int _position;

	protected BaseAccountViewModel(INotificationsService notificationService, ILogger logger): base(notificationService, logger)
	{
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
