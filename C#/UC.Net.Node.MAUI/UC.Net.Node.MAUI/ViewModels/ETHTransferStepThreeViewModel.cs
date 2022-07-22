namespace UC.Net.Node.MAUI.ViewModels;

public partial class ETHTransferStepThreeViewModel : BaseViewModel
{
    public ETHTransferStepThreeViewModel(ILogger<ETHTransferStepThreeViewModel> logger) : base(logger){}

    Wallet _Wallet = new Wallet
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };
    public Wallet Wallet
    {
        get { return _Wallet; }
        set { SetProperty(ref _Wallet, value); }
    }
}
