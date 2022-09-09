namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class Send2ViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _recipientWallet = DefaultDataMock.Wallet1;
    
	[ObservableProperty]
    private Wallet _sourceWallet = DefaultDataMock.Wallet2;

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
}
