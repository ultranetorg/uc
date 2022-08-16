namespace UC.Net.Node.MAUI.Popups;

public partial class SourceAccountPopup : Popup
{
	private static SourceAccountPopup popup;
	public SourceAccountViewModel vm => BindingContext as SourceAccountViewModel;

    public SourceAccountPopup(SourceAccountViewModel vm)
    {
        InitializeComponent();
		// TBR
		vm.Popup = this;
        BindingContext = vm;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task<Wallet> Show()
	{
		popup = new SourceAccountPopup(App.ServiceProvider.GetService<SourceAccountViewModel>());
		await App.Current.MainPage.ShowPopupAsync(popup);
		return popup.vm.Wallet;
	}
}
