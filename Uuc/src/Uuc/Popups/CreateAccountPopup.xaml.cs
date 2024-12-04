using CommunityToolkit.Maui.Views;
using Uuc.PageModels.Popups;

namespace Uuc.Popups;

public partial class CreateAccountPopup : Popup
{
	public CreateAccountPopup(CreateAccountPopupModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
