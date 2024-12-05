using Uuc.PageModels;

namespace Uuc.Pages;

public partial class DigitalIdentitiesPage
{
	public DigitalIdentitiesPage(DigitalIdentitiesPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
