namespace UC.Umc.ViewModels;

public partial class WhatsNewViewModel : BasePageViewModel
{
	[ObservableProperty]
    private List<string> _addedList = DefaultDataMock.AddedList;

	[ObservableProperty]
    private List<string> _fixedList = DefaultDataMock.FixedList;

    public WhatsNewViewModel(INotificationsService notificationService,
		ILogger<WhatsNewViewModel> logger) : base(notificationService, logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();
}