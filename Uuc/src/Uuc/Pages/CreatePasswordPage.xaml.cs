using Uuc.PageModels;

namespace Uuc.Pages;

public partial class CreatePasswordPage : ContentPage
{
	public CreatePasswordPage(CreatePasswordPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
