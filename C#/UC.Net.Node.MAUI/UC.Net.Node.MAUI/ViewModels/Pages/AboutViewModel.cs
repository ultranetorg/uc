namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AboutViewModel : BaseViewModel
{
    public AboutViewModel(ILogger<AboutViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }
	
	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}
