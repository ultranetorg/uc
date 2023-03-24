namespace UC.Umc.Views;

public class BasePageTitleView<TView> : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(TView));

    public static readonly BindableProperty TitleStyleProperty =
        BindableProperty.Create(nameof(TitleStyle), typeof(Style), typeof(TView));

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
}
