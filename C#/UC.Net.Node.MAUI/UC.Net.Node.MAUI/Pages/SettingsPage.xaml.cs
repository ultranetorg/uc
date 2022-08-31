namespace UC.Net.Node.MAUI.Pages;

public partial class SettingsPage : CustomPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<SettingsViewModel>();
    }

    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
