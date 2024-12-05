using Uuc.PageModels;

namespace Uuc.Pages;

public partial class AccountDetailsPage
{
	public AccountDetailsPage(AccountDetailsPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
