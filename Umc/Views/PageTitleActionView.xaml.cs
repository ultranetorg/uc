using System.Windows.Input;

namespace UC.Umc.Views;

public partial class PageTitleActionView : BasePageTitleView<PageTitleActionView>
{
    public static readonly BindableProperty NotificationsCountProperty = BindableProperty.Create(nameof(NotificationsCount), typeof(int), typeof(PageTitleActionView));

    public static readonly BindableProperty MaxSeverityProperty = BindableProperty.Create(nameof(MaxSeverity), typeof(Severity), typeof(PageTitleActionView));

	public static readonly BindableProperty NotificationsCommandProperty = BindableProperty.Create(nameof(NotificationsCommand), typeof(ICommand), typeof(PageTitleActionView));

    public static readonly BindableProperty ConnectionStateProperty = BindableProperty.Create(nameof(ConnectionState), typeof(NetworkAccess), typeof(PageTitleActionView));

	public static readonly BindableProperty IconSourceProperty = BindableProperty.Create(nameof(IconSource), typeof(string), typeof(PageTitleActionView));

	public static readonly BindableProperty IconStyleProperty = BindableProperty.Create(nameof(IconStyle), typeof(Style), typeof(PageTitleActionView));

	public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(PageTitleActionView));

    public int NotificationsCount
    {
        get => (int)GetValue(NotificationsCountProperty);
        set => SetValue(NotificationsCountProperty, value);
    }

    public Severity MaxSeverity
    {
        get => (Severity)GetValue(MaxSeverityProperty);
        set => SetValue(MaxSeverityProperty, value);
    }

	public ICommand NotificationsCommand
	{
		get => (ICommand)GetValue(NotificationsCommandProperty);
		set => SetValue(NotificationsCommandProperty, value);
	}

    public NetworkAccess ConnectionState
    {
        get => (NetworkAccess)GetValue(ConnectionStateProperty);
        set => SetValue(ConnectionStateProperty, value);
    }

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
