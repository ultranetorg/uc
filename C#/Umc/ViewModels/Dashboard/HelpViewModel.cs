using UC.Net;

namespace UC.Umc.ViewModels;

public partial class HelpViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private Transaction _selectedItem ;

	[ObservableProperty]
    private CustomCollection<string> _helps = new();

	[ObservableProperty]
    private string _filter;

    public HelpViewModel(IServicesMockData service, ILogger<HelpViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private async Task OpenDetailsAsync()
    {
		// need to pass question id through the query
        await Navigation.GoToAsync(Routes.HELP_DETAILS);
    }
	
	[RelayCommand]
    public async Task SearchHelpsAsync()
    {
		try
		{
			Guard.IsNotNull(Filter);

			InitializeLoading();

			// Search help questions
			var helps = _service.HelpQuestions.Where(x => x.Contains(Filter));

			await Task.Delay(10);
			
			Helps = new(helps);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchHelpsAsync Error: {Message}", ex.Message);
		}
    }

	private void LoadData()
	{
		Helps = new(_service.HelpQuestions);
	}
}
