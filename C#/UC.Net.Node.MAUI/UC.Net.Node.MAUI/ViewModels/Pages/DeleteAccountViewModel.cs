namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class DeleteAccountViewModel : BaseViewModel
{
	// will be splitted into 3 services
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<Product> _products = new();

	[ObservableProperty]
    private Wallet _wallet;

    public DeleteAccountViewModel(IServicesMockData service, ILogger<DeleteAccountViewModel> logger) : base(logger)
    {
		_service = service;
    }

    [RelayCommand]
    private async void DeleteAsync()
    {
        await DeleteAccountPopup.Show(Wallet);
    }

	internal void Initialize(Wallet wallet)
	{
		Wallet = wallet;
		Authors.Clear();
		Products.Clear();
		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
	}
}