using Uuc.PageModels;

namespace Uuc.Pages;

public partial class ImportWalletPage : ContentPage
{
	public ImportWalletPage(ImportWalletPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
