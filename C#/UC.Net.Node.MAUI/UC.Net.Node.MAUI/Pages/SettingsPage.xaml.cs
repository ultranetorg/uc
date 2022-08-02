namespace UC.Net.Node.MAUI.Pages;

public partial class SettingsPage : CustomPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel(ServiceHelper.GetService<ILogger<SettingsViewModel>>());
    }
}
