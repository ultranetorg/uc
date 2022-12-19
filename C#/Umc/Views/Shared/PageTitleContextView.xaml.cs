using System.Windows.Input;

namespace UC.Umc.Views.Shared;

public partial class PageTitleContextView : BasePageTitleView<PageTitleContextView>
{
	public static readonly BindableProperty Action1TextProperty =
		BindableProperty.Create(nameof(Action1Text), typeof(string), typeof(PageTitleContextView));

	public static readonly BindableProperty Action2TextProperty =
		BindableProperty.Create(nameof(Action2Text), typeof(string), typeof(PageTitleContextView));

	public static readonly BindableProperty Action3TextProperty =
		BindableProperty.Create(nameof(Action3Text), typeof(string), typeof(PageTitleContextView));

	public static readonly BindableProperty Tap1CommandProperty =
		BindableProperty.Create(nameof(Tap1Command), typeof(ICommand), typeof(PageTitleContextView));

	public static readonly BindableProperty Tap2CommandProperty =
		BindableProperty.Create(nameof(Tap2Command), typeof(ICommand), typeof(PageTitleContextView));

	public static readonly BindableProperty Tap3CommandProperty =
		BindableProperty.Create(nameof(Tap3Command), typeof(ICommand), typeof(PageTitleContextView));

	public string Action1Text
	{
		get => (string)GetValue(Action1TextProperty);
		set => SetValue(Action1TextProperty, value);
	}

	public string Action2Text
	{
		get => (string)GetValue(Action2TextProperty);
		set => SetValue(Action2TextProperty, value);
	}

	public string Action3Text
	{
		get => (string)GetValue(Action3TextProperty);
		set => SetValue(Action3TextProperty, value);
	}

	public ICommand Tap1Command
	{
		get => (ICommand)GetValue(Tap1CommandProperty);
		set => SetValue(Tap1CommandProperty, value);
	}

	public ICommand Tap2Command
	{
		get => (ICommand)GetValue(Tap2CommandProperty);
		set => SetValue(Tap2CommandProperty, value);
	}

	public ICommand Tap3Command
	{
		get => (ICommand)GetValue(Tap3CommandProperty);
		set => SetValue(Tap3CommandProperty, value);
	}

	public PageTitleContextView()
    {
        InitializeComponent();
    }
}
