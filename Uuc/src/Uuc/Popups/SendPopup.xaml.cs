using CommunityToolkit.Maui.Views;

namespace Uuc.Popups;

public partial class SendPopup : Popup
{
	public SendPopup(SendPopupModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
