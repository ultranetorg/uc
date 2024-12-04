using Uuc.PageModels;
using Uuc.PageModels.Base;

namespace Uuc.Pages;

public partial class DigitalIdentitiesPage : ContentPage
{
	public DigitalIdentitiesPage(DigitalIdentitiesPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
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
