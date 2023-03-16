namespace UO.Mobile.UUC.Controls;

public partial class ApplicationCard : ContentView
{
    public static readonly BindableProperty LogoSourceProperty =
        BindableProperty.Create(nameof(LogoSource), typeof(string), typeof(ApplicationCard), default(string));

    public static readonly BindableProperty ApplicationNameProperty =
        BindableProperty.Create(nameof(ApplicationName), typeof(string), typeof(ApplicationCard), default(string));

    public static readonly BindableProperty VersionProperty =
        BindableProperty.Create(nameof(Version), typeof(string), typeof(ApplicationCard), default(string));

    public ApplicationCard()
    {
        InitializeComponent();
    }

    public string LogoSource
    {
        get => (string) GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    public string ApplicationName
    {
        get => (string) GetValue(ApplicationNameProperty);
        set => SetValue(ApplicationNameProperty, value);
    }

    public string Version
    {
        get => (string) GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }
}
