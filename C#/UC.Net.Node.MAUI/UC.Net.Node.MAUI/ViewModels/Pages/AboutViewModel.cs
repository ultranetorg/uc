namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AboutViewModel : BaseViewModel
{
    public Page Page { get; }

    public AboutViewModel(Page page, ILogger<AboutViewModel> logger) : base(logger)
    {
        Page = page;
    }

	[RelayCommand]
    private async void Cancel()
    {
        await Page.Navigation.PopAsync();
    }
	
	[RelayCommand]
    private async void Transactions()
    {
        await Page.Navigation.PushAsync(new TransactionsPage());
    }
}
