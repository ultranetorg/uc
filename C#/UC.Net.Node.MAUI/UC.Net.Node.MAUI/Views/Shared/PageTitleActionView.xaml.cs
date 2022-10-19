using System.Windows.Input;

namespace UC.Net.Node.MAUI.Views.Shared;

public partial class PageTitleActionView : BasePageTitleView<PageTitleActionView>
{
    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(nameof(IconSource), typeof(string), typeof(PageTitleActionView));

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(PageTitleActionView));

    public string IconSource
    {
        get => (string)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public PageTitleActionView()
    {
        InitializeComponent();
    }
}
