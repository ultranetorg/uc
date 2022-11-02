namespace UC.Umc.ViewModels;

public partial class DeleteAccountViewModel : BaseAccountViewModel
{
	// will be splitted into 3 services
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<Product> _products = new();

    public DeleteAccountViewModel(IServicesMockData service, ILogger<DeleteAccountViewModel> logger) : base(logger)
    {
		_service = service;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        await DeleteAccountPopup.Show(Account);
    }

	internal void Initialize(AccountViewModel account)
	{
		Account = account;
		Authors.Clear();
		Products.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
	}
}