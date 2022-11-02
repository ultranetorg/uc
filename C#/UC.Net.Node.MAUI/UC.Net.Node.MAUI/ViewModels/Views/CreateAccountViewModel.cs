namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class CreateAccountViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    AccountColor _selectedAccountColor;

    public CreateAccountViewModel(IServicesMockData service, ILogger<CreateAccountViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void Randomize()
    {
        ColorTappedCommand.Execute(ColorsCollection[new Random().Next(0, ColorsCollection.Count)]);
    }

	private void LoadData()
	{
		// TODO: refactoring, get background color from resources
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);
        SelectedAccountColor = ColorsCollection.First();
	}
}
