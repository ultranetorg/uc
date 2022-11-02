namespace UC.Umc.ViewModels.Views;

public partial class Send1ViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _sourceAccount;

	[ObservableProperty]
    private AccountViewModel _recipientAccount;

    public Send1ViewModel(ILogger<Send1ViewModel> logger): base(logger)
	{
		LoadData();
	}
	
	private void LoadData()
	{
		SourceAccount = DefaultDataMock.CreateAccount();
		RecipientAccount = DefaultDataMock.CreateAccount();
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
