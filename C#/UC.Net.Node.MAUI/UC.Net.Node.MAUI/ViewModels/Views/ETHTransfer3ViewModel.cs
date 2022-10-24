namespace UC.Net.Node.MAUI.ViewModels;

public partial class ETHTransfer3ViewModel : BaseViewModel
{
	[ObservableProperty]
    private Account _account = DefaultDataMock.Account1;

    public ETHTransfer3ViewModel(ILogger<ETHTransfer3ViewModel> logger) : base(logger){}
}
