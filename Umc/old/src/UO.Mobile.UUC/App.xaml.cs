using UO.Mobile.UUC.Pages;
using Application = Microsoft.Maui.Controls.Application;

namespace UO.Mobile.UUC;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        NavigateToMainPageAsync();
    }

    private async void NavigateToMainPageAsync()
    {
        await Navigation.NavigateToAsync<MainPage>();
    }
}
