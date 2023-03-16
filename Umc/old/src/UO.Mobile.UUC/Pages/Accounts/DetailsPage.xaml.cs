using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.Pages.Accounts;

public partial class DetailsPage : ContentPage
{
    public DetailsPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        // TODO: create helper.
        if (BindingContext is IBackButtonPressedHandler handler)
        {
            handler.OnBackButtonPressed();
        }

        return base.OnBackButtonPressed();
    }
}
