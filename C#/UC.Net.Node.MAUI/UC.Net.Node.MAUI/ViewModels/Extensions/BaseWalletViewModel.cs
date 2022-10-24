namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseWalletViewModel : BaseViewModel
{
	[ObservableProperty]
    private Account _account = DefaultDataMock.Account1;

	[ObservableProperty]
    private Author _author = DefaultDataMock.Author1;

	protected BaseWalletViewModel(ILogger logger): base(logger){}

    [RelayCommand]
    private async Task SelectAuthorAsync()
    {
        var author = await SelectAuthorPopup.Show();
        if (author != null)
		{
			Author = author;
		}
    }

    [RelayCommand]
    private async Task SelectAccountAsync()
    {
        var account = await SourceAccountPopup.Show();
        if (account != null)
		{
			Account = account;
		}
    }
}
