namespace UC.Umc.ViewModels;

public partial class WhatsNewViewModel : BaseViewModel
{
    public WhatsNewViewModel(ILogger<WhatsNewViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync()
    {
        await Navigation.PopAsync();
    }
}