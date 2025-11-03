namespace UC.Umc.ViewModels;

public partial class HelpViewModel : BasePageViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<HelpInfo> _helps = new();

	[ObservableProperty]
    private string _filter;

    public HelpViewModel(INotificationsService notificationService, IServicesMockData service,
		ILogger<HelpViewModel> logger) : base(notificationService, logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();

	[RelayCommand]
    private async Task OpenDetailsAsync(HelpInfo info)
    {
        await Navigation.GoToAsync(Routes.HELP_DETAILS,
			new Dictionary<string, object>()
		{
			{ QueryKeys.HELP_INFO, info }
		});
    }
	
	[RelayCommand]
    public async Task SearchHelpsAsync()
    {
		try
		{
			Guard.IsNotNull(Filter);

			InitializeLoading();

			// Search help questions
			var helps = _service.HelpQuestions.Where(x => x.Question.Contains(Filter));

			await Task.Delay(10);
			
			Helps = new(helps);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			await ToastHelper.ShowDefaultErrorMessageAsync();
			_logger.LogError("SearchHelpsAsync Error: {Message}", ex.Message);
		}
    }

	private void LoadData()
	{
		Helps = new(_service.HelpQuestions);
	}
}
