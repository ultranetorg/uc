using System.Windows.Input;

namespace UC.Umc.Views.Shared;

public partial class PageTitleActionView : BasePageTitleView<PageTitleActionView>
{
    public static readonly BindableProperty IconLeftSourceProperty =
        BindableProperty.Create(nameof(IconLeftSource), typeof(string), typeof(PageTitleActionView));

    public static readonly BindableProperty IconLeftStyleProperty =
        BindableProperty.Create(nameof(IconLeftStyle), typeof(Style), typeof(PageTitleActionView));

    public static readonly BindableProperty TapLeftCommandProperty =
        BindableProperty.Create(nameof(TapLeftCommand), typeof(ICommand), typeof(PageTitleActionView));

	public static readonly BindableProperty IconRightSourceProperty =
		BindableProperty.Create(nameof(IconRightSource), typeof(string), typeof(PageTitleActionView));

	public static readonly BindableProperty IconRightStyleProperty =
		BindableProperty.Create(nameof(IconRightStyle), typeof(Style), typeof(PageTitleActionView));

	public static readonly BindableProperty TapRightCommandProperty =
		BindableProperty.Create(nameof(TapRightCommand), typeof(ICommand), typeof(PageTitleActionView));

	public string IconLeftSource
	{
        get => (string)GetValue(IconLeftSourceProperty);
        set => SetValue(IconLeftSourceProperty, value);
    }

    public Style IconLeftStyle
	{
        get => (Style)GetValue(IconLeftStyleProperty);
        set => SetValue(IconLeftStyleProperty, value);
    }

    public ICommand TapLeftCommand
	{
        get => (ICommand)GetValue(TapLeftCommandProperty);
        set => SetValue(TapLeftCommandProperty, value);
	}

	public string IconRightSource
	{
		get => (string)GetValue(IconRightSourceProperty);
		set => SetValue(IconRightSourceProperty, value);
	}

	public Style IconRightStyle
	{
		get => (Style)GetValue(IconRightStyleProperty);
		set => SetValue(IconRightStyleProperty, value);
	}

	public ICommand TapRightCommand
	{
		get => (ICommand)GetValue(TapRightCommandProperty);
		set => SetValue(TapRightCommandProperty, value);
	}

	public PageTitleActionView()
    {
        InitializeComponent();
    }
}
