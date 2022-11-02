namespace UC.Umc.ViewModels;

public partial class ETHTransfer3ViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

    public ETHTransfer3ViewModel(ILogger<ETHTransfer3ViewModel> logger) : base(logger)
	{
		LoadData();
	}
	
	private void LoadData()
	{
		Account = DefaultDataMock.CreateAccount();
	}
}
