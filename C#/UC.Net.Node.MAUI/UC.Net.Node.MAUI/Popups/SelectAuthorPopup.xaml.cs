namespace UC.Net.Node.MAUI.Popups;

public partial class SelectAuthorPopup : Popup
{
	private static SelectAuthorPopup popup;
	public SelectAuthorViewModel Vm => BindingContext as SelectAuthorViewModel;

    public SelectAuthorPopup()
    {
        InitializeComponent();
		// TBD: Popup manager
        BindingContext = Ioc.Default.GetService<SelectAuthorViewModel>();
		Vm.Popup = this;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task<Author> Show()
	{
		popup = new SelectAuthorPopup();
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
		return popup.Vm.SelectedAuthor;
	}
}