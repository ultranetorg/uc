using Uuc.PageModels;

namespace Uuc.Pages;

public partial class DigitalIdentityDetailsPage
{
	public DigitalIdentityDetailsPage(DigitalIdentityDetailsPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
