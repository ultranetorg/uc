namespace UC.Net.Node.MAUI.Popups;

public partial class AccountsPopup : Popup
{
    public AccountsPopup()
    {
        InitializeComponent();
		Size = PopupSizeConstants.AutoCompleteControl;
    }

    public void Hide()
    {
        Close();
    }
}