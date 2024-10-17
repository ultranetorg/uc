
using CommunityToolkit.Maui.Views;

namespace UC.Umc.Popups;

public partial class AlertPopup : Popup
{
	private static AlertPopup popup;

	public AlertPopup(string message)
	{
		InitializeComponent();

		Message = message;
		BindingContext = this;
		// Size = PopupSizeConstants.AutoCompleteControl;
	}

	public string Message { get; private set; }

	public void Hide()
	{
		Close();
	}
	
	public static async Task Show(string message)
	{
		popup = new AlertPopup(message);
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}

	private void CancelButtonClicked(object sender, EventArgs e)
	{
		Hide();
	}
}
