using Uuc.PageModels;
using Uuc.PageModels.Base;

namespace Uuc.Pages;

public partial class AccountsPage : ContentPage
{
	public AccountsPage(AccountsPageModel model)
	{
		BindingContext = model;
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is not IBasePageModel basePageModel)
		{
			return;
		}

		await basePageModel.InitializeAsyncCommand.ExecuteAsync(null);
	}
}
