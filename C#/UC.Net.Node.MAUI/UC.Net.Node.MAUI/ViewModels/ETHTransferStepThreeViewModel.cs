namespace UC.Net.Node.MAUI.ViewModels;

public partial class ETHTransferStepThreeViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _wallet = new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };

    public ETHTransferStepThreeViewModel(ILogger<ETHTransferStepThreeViewModel> logger) : base(logger){}
}
