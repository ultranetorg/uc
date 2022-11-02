namespace UC.Umc.Popups;

public partial class SourceAccountPopup : Popup
{
	private static SourceAccountPopup popup;
	public SourceAccountViewModel Vm => BindingContext as SourceAccountViewModel;

    public SourceAccountPopup()
    {
        InitializeComponent();
		// TBR
        BindingContext = Ioc.Default.GetService<SourceAccountViewModel>();
		Vm.Popup = this;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task<AccountViewModel> Show()
	{
		popup = new SourceAccountPopup();
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
		return popup.Vm.Account;
	}
}
