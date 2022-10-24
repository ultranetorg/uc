namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class Send2ViewModel : BaseViewModel
{
	[ObservableProperty]
    private Account _recipientAccount = DefaultDataMock.Account1;
    
	[ObservableProperty]
    private Account _sourceAccount = DefaultDataMock.Account2;

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
