namespace UC.Umc.ViewModels.Views;

public partial class Send2ViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _recipientAccount;
    
	[ObservableProperty]
    private AccountViewModel _sourceAccount;

    public Send2ViewModel(ILogger<Send2ViewModel> logger): base(logger)
	{
		LoadData();
	}
	
	private void LoadData()
	{
		SourceAccount = DefaultDataMock.CreateAccount();
		RecipientAccount = DefaultDataMock.CreateAccount();
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
