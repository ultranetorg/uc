namespace UC.Net.Node.MAUI.ViewModels.Pages;

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
    private async void AuthorTappedAsync(Author Author)
    {
        if (Author == null) return;
        await Shell.Current.Navigation.PushAsync(new AuthorSearchPage(Author));
    }
	
	[RelayCommand]
    private async void TransferAuthorAsync()
    {
        await Shell.Current.Navigation.PushAsync(new AuthorRegistrationPage());
    }
	
	[RelayCommand]
    private async void MakeBidAsync()
    {
        await Shell.Current.Navigation.PushAsync(new MakeBidPage());
    }

	public async Task InitializeAsync()
	{
        AuthorsFilter = new CustomCollection<string> {"All", "To be expired", "Expired", "Hidden", "Shown" };
		
		Authors.Clear();
		var authors = await _service.GetAllAsync();
		Authors.AddRange(authors);
	}
}
