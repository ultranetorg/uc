namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ETHTransferViewModel : BaseAccountViewModel
{
    public ETHTransferViewModel(ILogger<ETHTransferViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async void ConfirmAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransferCompletePage());
    }
}
