namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseWalletViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _wallet = new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };

	[ObservableProperty]
    private Author _author = new() { BidStatus = BidStatus.None, Name = "amazon.com", ActiveDue = "Active due: 07/07/2022 (in 182 days)" };

	protected BaseWalletViewModel(ILogger logger): base(logger){}

    [RelayCommand]
    private async void SelectAuthorAsync()
    {
        var author = await SelectAuthorPopup.Show();
        if (author != null)
		{
			Author = author;
		}
    }

    [RelayCommand]
    private async void SelectAccountAsync()
    {
        var wallet = await SourceAccountPopup.Show();
        if (wallet != null)
		{
			Wallet = wallet;
		}
    }
}
