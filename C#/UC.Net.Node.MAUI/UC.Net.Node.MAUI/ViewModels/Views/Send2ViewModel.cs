namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class Send2ViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _recipientWallet = new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };

    public Send2ViewModel(ILogger<Send2ViewModel> logger): base(logger)
	{
	}
        
	[RelayCommand]
    private void SourceTapped()
	{
	}

	[RelayCommand]
    private void RecipientTapped()
	{
	}
    
	[ObservableProperty]
    private Wallet _sourceWallet = new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47FO",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#56d7de"),
    };
}
