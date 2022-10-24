namespace UC.Net.Node.MAUI.ViewModels.Views;

public partial class Send1ViewModel : BaseViewModel
{
	[ObservableProperty]
    private Account _sourceAccount = DefaultDataMock.Account1;

	[ObservableProperty]
    private Account _recipientAccount = DefaultDataMock.Account2;

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
