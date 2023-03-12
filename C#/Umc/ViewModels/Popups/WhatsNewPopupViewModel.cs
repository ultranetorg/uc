namespace UC.Umc.ViewModels.Popups;

public partial class WhatsNewPopupViewModel : BaseViewModel
{
	[ObservableProperty]
    private List<string> _addedList = DefaultDataMock.AddedList;
	[ObservableProperty]
    private List<string> _fixedList = DefaultDataMock.FixedList;

    public WhatsNewPopupViewModel(ILogger<WhatsNewPopupViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task ContinueAsync()
    {
        await Navigation.PopAsync();
    }
}