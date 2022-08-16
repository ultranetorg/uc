using UC.Net.Node.MAUI.Models.Constants;
namespace UC.Net.Node.MAUI.Popups;

public partial class NoNetworkPopup : Popup
{
	private static NoNetworkPopup popup;

    public NoNetworkPopup()
    {
        InitializeComponent();
		Size = PopupSizeConstants.AutoCompleteControl;
    }

    public void Hide()
    {
        Close();
    }
	
	// this will be static helper method
	public static async Task Show()
    {
        popup = new NoNetworkPopup();
        await App.Current.MainPage.ShowPopupAsync(popup);
    }
}