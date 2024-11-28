using CommunityToolkit.Maui.Views;

namespace Uuc.Popups;

public partial class ReceivePopup : Popup
{
	public ReceivePopup(ReceivePopupModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
