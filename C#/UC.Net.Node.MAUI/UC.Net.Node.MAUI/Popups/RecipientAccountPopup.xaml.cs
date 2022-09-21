namespace UC.Net.Node.MAUI.Popups;

public partial class RecipientAccountPopup : Popup
{
	private static RecipientAccountPopup popup;

    public RecipientAccountPopup(RecipientAccountViewModel vm)
    {
        InitializeComponent();
		vm.Popup = this;
        BindingContext = vm;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task Show()
	{
		popup = new RecipientAccountPopup(Ioc.Default.GetService<RecipientAccountViewModel>());
		await App.Current.MainPage.ShowPopupAsync(popup);
	}
}