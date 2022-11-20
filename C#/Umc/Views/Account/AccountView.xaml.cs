namespace UC.Umc.Views;

public partial class AccountView : ContentView
{
    public static readonly BindableProperty AccountProperty =
		BindableProperty.Create(nameof(Account), typeof(AccountViewModel), typeof(BadgeView));

    public AccountViewModel Account
    {
        get { return (AccountViewModel)GetValue(AccountProperty); }
        set { SetValue(AccountProperty, value); }
    }

    public AccountView()
    {
        InitializeComponent();
    }
}