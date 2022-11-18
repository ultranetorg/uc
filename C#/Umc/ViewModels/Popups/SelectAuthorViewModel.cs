namespace UC.Umc.ViewModels.Popups;

public partial class SelectAuthorViewModel : BaseViewModel
{
	private readonly IAuthorsService _service;

    public Author SelectedAuthor;

	[ObservableProperty]
	private CustomCollection<Author> _authors = new();

    public SelectAuthorViewModel(IAuthorsService service, ILogger<SelectAuthorViewModel> logger) : base(logger)
    {
		Initialize();
		_service = service;
    }

	[RelayCommand]
    private void ItemTapped(Author Author)
    {
        SelectedAuthor = Author;
    }

	[RelayCommand]
    private void Close() => ClosePopup();
	
	public void Initialize()
	{
		var authors = Task.Run(async () => await _service.GetAllAsync()).Result;
		Authors.AddRange(authors);
	}
}
