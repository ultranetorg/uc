namespace UC.Net.Node.MAUI.Popups;

public partial class SelectAuthorPopup : Popup
{
	private static SelectAuthorPopup popup;
	public SelectAuthorViewModel vm => BindingContext as SelectAuthorViewModel;

    public SelectAuthorPopup()
    {
        InitializeComponent();
        BindingContext = new SelectAuthorViewModel(this, ServiceHelper.GetService<ILogger<SelectAuthorViewModel>>());
    }

    public void Hide()
    {
		Close();
    }

	public static async Task<Author> Show()
	{
		popup = new SelectAuthorPopup();
		await App.Current.MainPage.ShowPopupAsync(popup);
		return popup.vm.SelectedAuthor;
	}
}