namespace UC.Umc.Controls;

public partial class EmptyView : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(EmptyView), null);

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public EmptyView()
    {
        InitializeComponent();
    }
}