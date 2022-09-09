namespace UC.Net.Node.MAUI.ViewModels;

public partial class ETHTransfer3ViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

    public ETHTransfer3ViewModel(ILogger<ETHTransfer3ViewModel> logger) : base(logger){}
}
