namespace UC.Net.Node.MAUI.ViewModels;

public partial class ETHTransferViewModel : BaseAccountViewModel
{
    public ETHTransferViewModel(ILogger<ETHTransferViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task ConfirmAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransferCompletePage());
    }
}
