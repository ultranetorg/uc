namespace UC.Net.Node.MAUI.Views;
public partial class BadgeView : ContentView
{
    public static readonly BindableProperty BadgeValueProperty = BindableProperty.Create("BadgeValue", typeof(string), typeof(BadgeView), string.Empty);
    public string BadgeValue
    {
        get { return (string)GetValue(BadgeValueProperty); }
        set { SetValue(BadgeValueProperty, value); }
    }
    public BadgeView()
    {
        InitializeComponent();
    }
}