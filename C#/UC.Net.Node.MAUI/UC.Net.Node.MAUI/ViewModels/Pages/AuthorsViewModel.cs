namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AuthorsViewModel : BaseTransactionsViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private Author _selectedItem;

	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<string> _authorsFilter = new();

    public AuthorsViewModel(IServicesMockData service, ILogger<AuthorsViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
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

	private void LoadData()
	{
        AuthorsFilter = new CustomCollection<string> {"All", "To be expired", "Expired", "Hidden", "Shown" };

		Authors.Clear();
		Authors.AddRange(_service.Authors);
	}
}
