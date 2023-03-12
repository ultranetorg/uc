namespace UC.Umc.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
    public AboutViewModel(ILogger<AboutViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync()
    {
		// Some action, e.g. opening the browser
        await Task.Delay(10);
    }
}
