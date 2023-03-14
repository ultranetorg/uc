namespace UC.Umc.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
    public AboutViewModel(ILogger<AboutViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();

	[RelayCommand]
    private async Task SomeActionAsync()
    {
		// Some action, e.g. opening the browser
        await Task.Delay(10);
    }
}
