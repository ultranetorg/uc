namespace UC.Umc.Views;

public partial class AccountView : ContentView
{
    public static readonly BindableProperty IconCodeProperty =
		BindableProperty.Create(nameof(IconCode), typeof(string), typeof(BadgeView), string.Empty);

    public string IconCode
    {
        get { return (string)GetValue(IconCodeProperty); }
        set { SetValue(IconCodeProperty, value); }
    }

    public AccountView()
    {
        InitializeComponent();
    }
}