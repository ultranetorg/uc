using Uuc.PageModels;

namespace Uuc.Pages;

public partial class AccountsPage
{
	public AccountsPage(AccountsPageModel model)
	{
		BindingContext = model;
		InitializeComponent();
	}
}
