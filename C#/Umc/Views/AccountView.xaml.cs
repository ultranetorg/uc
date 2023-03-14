namespace UC.Umc.Views;

public partial class AccountView : ContentView
{
    public static readonly BindableProperty AccountProperty =
		BindableProperty.Create(nameof(Account), typeof(AccountViewModel), typeof(AccountView));

	public static readonly BindableProperty BorderStyleProperty =
		BindableProperty.Create(nameof(BorderStyle), typeof(Style), typeof(AccountView));

	public AccountViewModel Account
    {
        get { return (AccountViewModel)GetValue(AccountProperty); }
        set { SetValue(AccountProperty, value); }
	}

	public Style BorderStyle
	{
		get { return (Style)GetValue(BorderStyleProperty); }
		set { SetValue(BorderStyleProperty, value); }
	}

	public AccountView()
    {
        InitializeComponent();
    }
}