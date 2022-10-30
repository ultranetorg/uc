namespace UC.Net.Node.MAUI.Popups;

public partial class SourceAccountPopup : Popup
{
	private static SourceAccountPopup popup;
	public SourceAccountViewModel Vm => BindingContext as SourceAccountViewModel;

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

	public static async Task<AccountViewModel> Show()
	{
		popup = new SourceAccountPopup(Ioc.Default.GetService<SourceAccountViewModel>());
		await App.Current.MainPage.ShowPopupAsync(popup);
		return popup.Vm.Account;
	}
}
