using System.Windows.Input;

namespace UC.Umc.Views.Shared;

public partial class PageTitleActionView : BasePageTitleView<PageTitleActionView>
{
	public static readonly BindableProperty IconSourceProperty =
		BindableProperty.Create(nameof(IconSource), typeof(string), typeof(PageTitleActionView));

	public static readonly BindableProperty IconStyleProperty =
		BindableProperty.Create(nameof(IconStyle), typeof(Style), typeof(PageTitleActionView));

	public static readonly BindableProperty TapCommandProperty =
		BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(PageTitleActionView));

	public string IconSource
	{
		get => (string)GetValue(IconSourceProperty);
		set => SetValue(IconSourceProperty, value);
	}

	public Style IconStyle
	{
		get => (Style)GetValue(IconStyleProperty);
		set => SetValue(IconStyleProperty, value);
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
