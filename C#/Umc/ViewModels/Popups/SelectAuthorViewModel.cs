namespace UC.Umc.ViewModels.Popups;

public partial class SelectAuthorViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

    public AuthorViewModel SelectedAuthor;

	[ObservableProperty]
	private CustomCollection<AuthorViewModel> _authors = new();

    public SelectAuthorViewModel(IServicesMockData service, ILogger<SelectAuthorViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void ItemTapped(AuthorViewModel Author)
    {
        SelectedAuthor = Author;
    }

	[RelayCommand]
    private void Close() => ClosePopup();
	
	public void LoadData()
	{
		Authors.Clear();
		var ownAuthors = _service.Authors.Where(x => x.Status == AuthorStatus.Owned).ToList();
		Authors.AddRange(ownAuthors);
	}
}
