using UC.Net.Node.MAUI.Models.Constants;
namespace UC.Net.Node.MAUI.Popups;

public partial class AlertPopup : Popup
{
	private static AlertPopup popup;

    public AlertPopup(string message)
    {
        InitializeComponent();

        Message = message;
		Size = PopupSizeConstants.AutoCompleteControl;
        BindingContext = this;
    }

    public string Message { get; private set; }

    public void Hide()
    {
        Close();
    }
	
    public static async Task Show(string message)
    {
        popup = new AlertPopup(message);
        await App.Current.MainPage.ShowPopupAsync(popup);
    }

    private void CancelButtonClicked(object sender, EventArgs e)
    {
        Hide();
    }
}