namespace UC.Umc.ViewModels;

public partial class AuthorsViewModel : BaseTransactionsViewModel
{
	private readonly IAuthorsService _service;

	[ObservableProperty]
    private string _search;

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
    public async Task SearchAuthorsAsync()
    {
		try
		{
			Guard.IsNotNull(Search);

			InitializeLoading();

			// Search authors
			var authors = await _service.SearchAuthorsAsync(Search);

			Authors.Clear();
			Authors.AddRange(authors);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchAuthorsAsync Error: {Message}", ex.Message);
		}
    }
	
	[RelayCommand]
    public async Task FilterAuthorsAsync(string status)
    {
		try
		{
			Guard.IsNotNull(status);

			InitializeLoading();

			// Filter authors
			ObservableCollection<AuthorViewModel> authors;
			if (status != string.Empty && status != "All")
			{
				var authorStatus = (AuthorStatus)Enum.Parse(typeof(AuthorStatus), status);
				authors = await _service.FilterAuthorsAsync(authorStatus);
			}
			else
			{
				authors = await _service.GetAccountAuthorsAsync();
			}
			
			Authors.Clear();
			Authors.AddRange(authors);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchAuthorsAsync Error: {Message}", ex.Message);
		}
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
		InitializeLoading();
		
		Authors.Clear();
		var authors = await _service.GetAccountAuthorsAsync();
		Authors.AddRange(authors);
		
		FinishLoading();
	}
}
