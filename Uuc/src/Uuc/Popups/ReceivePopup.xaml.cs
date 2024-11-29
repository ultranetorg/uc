using CommunityToolkit.Maui.Views;
using Uuc.PageModels.Popups;

namespace Uuc.Popups;

public partial class ReceivePopup : Popup
{
	public ReceivePopup(ReceivePopupModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
