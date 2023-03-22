namespace UC.Umc.Views.Shared;

public partial class PageTitleNotificationsView : BasePageTitleView<PageTitleNotificationsView>
{
    public static readonly BindableProperty NotificationsCountProperty =
        BindableProperty.Create(nameof(NotificationsCount), typeof(int), typeof(PageTitleNotificationsView));

    public static readonly BindableProperty MaxSeverityProperty =
        BindableProperty.Create(nameof(MaxSeverity), typeof(Severity), typeof(PageTitleNotificationsView));

    public static readonly BindableProperty ConnectionIconProperty =
        BindableProperty.Create(nameof(ConnectionIcon), typeof(string), typeof(PageTitleNotificationsView));

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

    public string ConnectionIcon
    {
        get => (string)GetValue(ConnectionIconProperty);
        set => SetValue(ConnectionIconProperty, value);
    }

    public PageTitleNotificationsView()
    {
        InitializeComponent();
    }
}
