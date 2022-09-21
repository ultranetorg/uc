namespace UC.Net.Node.MAUI.Popups;

public partial class SelectAuthorPopup : Popup
{
	private static SelectAuthorPopup popup;
	public SelectAuthorViewModel vm => BindingContext as SelectAuthorViewModel;

    public SelectAuthorPopup(SelectAuthorViewModel vm)
    {
        InitializeComponent();
		// TBD: Popup manager
		vm.Popup = this;
        BindingContext = vm;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task<Author> Show()
	{
		popup = new SelectAuthorPopup(Ioc.Default.GetService<SelectAuthorViewModel>());
		await App.Current.MainPage.ShowPopupAsync(popup);
		return popup.vm.SelectedAuthor;
	}
}