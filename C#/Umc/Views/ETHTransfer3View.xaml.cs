namespace UC.Umc.Views;

public partial class ETHTransfer3View : ContentView
{
    public static readonly BindableProperty AccountProperty =
		BindableProperty.Create(nameof(Account), typeof(AccountViewModel), typeof(AccountView));

	public AccountViewModel Account
    {
        get { return (AccountViewModel)GetValue(AccountProperty); }
        set { SetValue(AccountProperty, value); }
	}

    public ETHTransfer3View()
    {
        InitializeComponent();
    }
}
