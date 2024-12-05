using Uuc.PageModels.Base;

namespace Uuc.Pages;

public abstract class BaseContentPage : ContentPage
{
	//public ContentPageBase()
	//{
	//	NavigationPage.SetBackButtonTitle(this, string.Empty);
	//}

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
