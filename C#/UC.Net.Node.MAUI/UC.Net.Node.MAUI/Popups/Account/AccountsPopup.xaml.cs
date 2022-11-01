using UC.Net.Node.MAUI.Constants;

namespace UC.Net.Node.MAUI.Popups;

public partial class AccountsPopup : Popup
{
    private static AccountsPopup popup;

    public AccountsPopup()
    {
        InitializeComponent();
		// Size = PopupSizeConstants.AutoCompleteControl;
    }

    public void Hide()
    {
        Close();
    }

    public static async Task Show()
    {
        popup = new AccountsPopup();
        await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
    }
}