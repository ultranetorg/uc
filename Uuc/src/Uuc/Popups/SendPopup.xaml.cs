using CommunityToolkit.Maui.Views;
using Uuc.PageModels.Popups;

namespace Uuc.Popups;

public partial class SendPopup : Popup
{
	public SendPopup(SendPopupModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
