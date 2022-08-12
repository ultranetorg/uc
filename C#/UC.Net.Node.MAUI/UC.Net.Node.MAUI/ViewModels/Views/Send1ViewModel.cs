namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class Send1ViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _sourceWallet = new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47FO",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#56d7de"),

    };
	[ObservableProperty]
    private Wallet _recipientWallet = new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };

    public Send1ViewModel(ILogger<Send1ViewModel> logger): base(logger){}
    
	[RelayCommand]
    private async void SourceTapped()
    {
        await SourceAccountPopup.Show();
    }

	[RelayCommand]
    private async void RecipientTapped()
    {
        await RecipientAccountPopup.Show();
    }
}
