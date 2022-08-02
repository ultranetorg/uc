namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class WhatsNewViewModel : BaseViewModel
{
    public WhatsNewViewModel(ILogger<WhatsNewViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async void CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }
	
	[RelayCommand]
    private async void TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}