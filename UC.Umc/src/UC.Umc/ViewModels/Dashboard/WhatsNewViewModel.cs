using UC.Umc.Common.Helpers;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Dashboard;

public partial class WhatsNewViewModel(ILogger<WhatsNewViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private List<string> _addedList = DefaultDataMock.AddedList;
	[ObservableProperty]
	private List<string> _fixedList = DefaultDataMock.FixedList;

	[RelayCommand]
	private async Task CancelAsync() => await Navigation.BackToDashboardAsync();
}