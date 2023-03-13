namespace UC.Umc.ViewModels;

public partial class WhatsNewViewModel : BaseViewModel
{
	[ObservableProperty]
    private List<string> _addedList = DefaultDataMock.AddedList;
	[ObservableProperty]
    private List<string> _fixedList = DefaultDataMock.FixedList;

    public WhatsNewViewModel(ILogger<WhatsNewViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();
}