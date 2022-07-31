namespace UC.Net.Node.MAUI.Popups;

public partial class SourceAccountPopup : Popup
{
	private static SourceAccountPopup popup;
	public SourceAccountViewModel vm => BindingContext as SourceAccountViewModel;

    public SourceAccountPopup()
    {
        InitializeComponent();
        BindingContext = new SourceAccountViewModel(this, ServiceHelper.GetService<ILogger<SourceAccountViewModel>>());
    }

    public void Hide()
    {
		Close();
    }

	public static async Task<Wallet> Show()
	{
		popup = new SourceAccountPopup();
		await App.Current.MainPage.ShowPopupAsync(popup);
		return popup.vm.Wallet;
	}
}
