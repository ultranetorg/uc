namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class SendViewModel : BaseViewModel
{
    public Page Page { get; }
      
	[ObservableProperty]
    private int _position;

    public SendViewModel(Page page, ILogger<SendViewModel> logger) : base(logger)
    {
        Page = page;
    }

	[RelayCommand]
    private async void Close()
    {
        await Page.Navigation.PopModalAsync();
    }

	[RelayCommand]
    private async void Confirm()
    {
		// new TransferCompletePage()
        await Page.Navigation.PushAsync(Page);
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
