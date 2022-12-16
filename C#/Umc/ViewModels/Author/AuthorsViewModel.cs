namespace UC.Umc.ViewModels;

public partial class AuthorsViewModel : BaseTransactionsViewModel
{
	private readonly IAuthorsService _service;

	[ObservableProperty]
    private Author _selectedItem;

	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<string> _authorsFilter = new();

    public AuthorsViewModel(IAuthorsService service, ILogger<AuthorsViewModel> logger) : base(logger)
    {
		_service = service;
    }
	
	[RelayCommand]
    private async Task AuthorTappedAsync(Author author) =>
		await Navigation.GoToAsync(nameof(AuthorSearchPage),
			new Dictionary<string, object>(){{ QueryKeys.AUTHOR, author }});
	
	[RelayCommand]
    private async Task TransferAuthorAsync() => await Navigation.GoToAsync(ShellBaseRoutes.AUTHOR_REGISTRATION);
	
	[RelayCommand]
    private async Task MakeBidAsync() => await Navigation.GoToAsync(ShellBaseRoutes.MAKE_BID);

	[RelayCommand]
    private async Task OpenAuthorOptionsAsync(Author author)
    {
        // await AccountOptionsPopup.Show(author);
		await Task.Delay(10);
    }

	public async Task InitializeAsync()
	{
        AuthorsFilter = DefaultDataMock.DefaultFilter;
		
		Authors.Clear();
		var authors = await _service.GetAllAsync();
		Authors.AddRange(authors);
	}
}
