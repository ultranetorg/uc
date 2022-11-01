namespace UC.Net.Node.MAUI.Popups;

public partial class RecipientAccountPopup : Popup
{
	private static RecipientAccountPopup popup;
	public RecipientAccountViewModel Vm => BindingContext as RecipientAccountViewModel;

    public RecipientAccountPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<RecipientAccountViewModel>();
		Vm.Popup = this;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task Show()
	{
		popup = new RecipientAccountPopup();
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}
}