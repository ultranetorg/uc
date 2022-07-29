namespace UC.Net.Node.MAUI.Popups;

public partial class SelectAuthorPopup : Popup
{
    public SelectAuthorPopup()
    {
        InitializeComponent();
        BindingContext = new SelectAuthorViewModel(this, ServiceHelper.GetService<ILogger<SelectAuthorViewModel>>());
    }

    public void Hide()
    {
		Close();
    }

    //public static async Task<Author> Show()
    //{
    //    popup = new SelectAuthorPopup();
    //    await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
    //    return popup.viewModel.SelectedAuthor;
    //}
}