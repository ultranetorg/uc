using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.Pages.Accounts;

public partial class AccountsPage : ContentPage
{
    public AccountsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is IInitializableAsync initializable)
        {
            if (!initializable.IsInitialized || initializable.MultipleInitialization)
            {
                await initializable.InitializeAsync(null);
            }
        }
    }
}
