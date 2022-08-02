namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AboutViewModel : BaseViewModel
{
    public AboutViewModel(ILogger<AboutViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async void Cancel()
    {
        await Shell.Current.Navigation.PopAsync();
    }
	
	[RelayCommand]
    private async void Transactions()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}
