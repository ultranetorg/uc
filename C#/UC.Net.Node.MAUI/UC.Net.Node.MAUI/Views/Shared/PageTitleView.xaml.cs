namespace UC.Net.Node.MAUI.Views.Shared;

public partial class PageTitleView : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageTitleView));

    public static readonly BindableProperty TitleStyleProperty =
        BindableProperty.Create(nameof(TitleStyle), typeof(Style), typeof(PageTitleView));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public Style TitleStyle
    {
        get => (Style)GetValue(TitleStyleProperty);
        set => SetValue(TitleStyleProperty, value);
    }

    public PageTitleView()
    {
        InitializeComponent();
    }
}
