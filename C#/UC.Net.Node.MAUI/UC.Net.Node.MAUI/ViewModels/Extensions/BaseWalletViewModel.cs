namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseWalletViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

	[ObservableProperty]
    private Author _author = DefaultDataMock.Author1;

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
