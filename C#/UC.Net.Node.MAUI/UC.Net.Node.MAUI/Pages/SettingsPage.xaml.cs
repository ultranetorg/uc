namespace UC.Net.Node.MAUI.Pages;

public partial class SettingsPage : CustomPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
