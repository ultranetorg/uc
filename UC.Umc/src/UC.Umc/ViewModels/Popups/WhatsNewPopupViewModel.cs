using UC.Umc.Common.Helpers;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Popups;

public partial class WhatsNewPopupViewModel(ILogger<WhatsNewPopupViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private List<string> _addedList = DefaultDataMock.AddedList;
	[ObservableProperty]
	private List<string> _fixedList = DefaultDataMock.FixedList;

	[RelayCommand]
	private async Task ContinueAsync() => await Navigation.BackToDashboardAsync();
}
