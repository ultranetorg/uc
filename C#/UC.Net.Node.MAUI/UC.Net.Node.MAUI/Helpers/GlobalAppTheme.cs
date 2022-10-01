namespace UC.Net.Node.MAUI.Helpers;

public static class GlobalAppTheme
{
    private const AppTheme THEME_OPTIONS = AppTheme.Unspecified;

    public static AppTheme Theme
    {
        get => Enum.Parse<AppTheme>(Preferences.Get(nameof(Theme), Enum.GetName(THEME_OPTIONS)));
        set => Preferences.Set(nameof(Theme), value.ToString());
    }
    public static bool IsWifiOnlyEnabled
    {
        get => Preferences.Get(nameof(IsWifiOnlyEnabled), false);
        set => Preferences.Set(nameof(IsWifiOnlyEnabled), value);
    }

    static GlobalAppTheme()
    {
        App.Current.RequestedThemeChanged += (s, a) =>
        {
            Theme = a.RequestedTheme;
        };
    }

    public static void SetTheme()
    {
        App.Current.UserAppTheme = Theme switch
        {
            AppTheme.Light => AppTheme.Light,
            AppTheme.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}