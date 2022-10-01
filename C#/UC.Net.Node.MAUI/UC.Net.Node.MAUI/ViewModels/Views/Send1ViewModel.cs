namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class Send1ViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _sourceWallet = DefaultDataMock.Wallet1;

	[ObservableProperty]
    private Wallet _recipientWallet = DefaultDataMock.Wallet2;

    public Send1ViewModel(ILogger<Send1ViewModel> logger): base(logger)
	{
	}
    
	[RelayCommand]
    private async Task SourceTapped()
    {
        await SourceAccountPopup.Show();
    }

	[RelayCommand]
    private async Task RecipientTapped()
    {
        await RecipientAccountPopup.Show();
    }
}
