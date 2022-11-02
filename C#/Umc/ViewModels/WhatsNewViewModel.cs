namespace UC.Umc.ViewModels;

public partial class WhatsNewViewModel : BaseViewModel
{
    public WhatsNewViewModel(ILogger<WhatsNewViewModel> logger) : base(logger)
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