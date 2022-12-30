namespace UC.Umc.ViewModels;

public partial class AuthorsViewModel : BaseTransactionsViewModel
{
	private readonly IAuthorsService _service;

	[ObservableProperty]
    private string _filter;

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
    private async Task OpenOptionsAsync(AuthorViewModel author)
	{
		try
		{
			Guard.IsNotNull(author);

			if (author.Status != AuthorStatus.Reserved)
			{
				await ShowPopup(new AuthorOptionsPopup(author));
			}
		}
		catch(ArgumentException ex)
		{
			_logger.LogError("OpenOptionsAsync: Author cannot be null, Error: {Message}", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenOptionsAsync Error: {Message}", ex.Message);
		}
	}
	
	[RelayCommand]
    private async Task OpenAuthorDetailsAsync(AuthorViewModel author) =>
		await Navigation.GoToAsync(nameof(AuthorDetailsPage),
			new Dictionary<string, object>(){{ QueryKeys.AUTHOR, author }});

	[RelayCommand]
	private async Task SortAuthorsAsync()
    {
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
