namespace UC.Umc.ViewModels.Popups;

public partial class SelectAuthorViewModel : BaseViewModel
{
	private readonly IAuthorsService _service;

    public AuthorViewModel SelectedAuthor;

	[ObservableProperty]
	private CustomCollection<AuthorViewModel> _authors = new();

    public SelectAuthorViewModel(IAuthorsService service, ILogger<SelectAuthorViewModel> logger) : base(logger)
    {
		Initialize();
		_service = service;
    }

	[RelayCommand]
    private void ItemTapped(AuthorViewModel Author)
    {
        SelectedAuthor = Author;
    }

	[RelayCommand]
    private void Close() => ClosePopup();
	
	public void Initialize()
	{
		var authors = _service.GetAccountAuthorsAsync().Result;
		Authors.AddRange(authors);
	}
}
