namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class SendViewModel : BaseViewModel
{
	[ObservableProperty]
    private int _position;

    public SendViewModel(ILogger<SendViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async void CloseAsync()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

	[RelayCommand]
    private async void ConfirmAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransferCompletePage());
    }
      
	[RelayCommand]
    private void Transfer()
    {
        if (Position != 1) 
		{
			Position += 1;
		} 
    }
}
