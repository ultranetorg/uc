namespace UC.Umc.ViewModels;

public partial class AuthorsViewModel : BaseTransactionsViewModel
{
	private readonly IAuthorsService _service;

	[ObservableProperty]
    private AuthorViewModel _selectedItem;

	[ObservableProperty]
    private CustomCollection<AuthorViewModel> _authors = new();

	[ObservableProperty]
    private CustomCollection<string> _authorsFilter = new();

    public AuthorsViewModel(IAuthorsService service, ILogger<AuthorsViewModel> logger) : base(logger)
    {
		_service = service;
    }
	
	[RelayCommand]
    public void FilterAuthorsAsync()
    {
        _logger.LogDebug("Filter Authors");

        // Filtering, Loading
    }
	
	[RelayCommand]
    private async Task OpenAuthorDetailsAsync(AuthorViewModel author) =>
		await Navigation.GoToAsync(nameof(AuthorDetailsPage),
			new Dictionary<string, object>(){{ QueryKeys.AUTHOR, author }});
	
	[RelayCommand]
    private async Task RegisterAuthorAsync() => await Navigation.GoToAsync(ShellBaseRoutes.AUTHOR_REGISTRATION);
	
	[RelayCommand]
    private async Task MakeBidAsync() => await Navigation.GoToAsync(ShellBaseRoutes.MAKE_BID);

	[RelayCommand]
	private async Task SortAuthorsAsync()
    {
		await Task.Delay(10);
    }

	[RelayCommand]
    private async Task TransferAuthorAsync()
    {
        // await AccountOptionsPopup.Show(author);
		await Task.Delay(10);
    }

	public async Task InitializeAsync()
	{
        AuthorsFilter = DefaultDataMock.AuthorsFilter;
		
		Authors.Clear();
		var authors = await _service.GetAccountAuthorsAsync();
		Authors.AddRange(authors);

		IsLoaded = true;
	}
}
