using System.Windows.Input;

namespace UC.Umc.Views;

public partial class ETHTransfer2View : ContentView
{
    public static readonly BindableProperty AccountProperty =
		BindableProperty.Create(nameof(Account), typeof(AccountViewModel), typeof(AccountView));

    public static readonly BindableProperty SelectAccountCommandProperty =
		BindableProperty.Create(nameof(SelectAccountCommand), typeof(ICommand), typeof(AccountView));

	public AccountViewModel Account
    {
        get { return (AccountViewModel)GetValue(AccountProperty); }
        set { SetValue(AccountProperty, value); }
	}

	public ICommand SelectAccountCommand
    {
        get { return (ICommand)GetValue(SelectAccountCommandProperty); }
        set { SetValue(SelectAccountCommandProperty, value); }
	}

    public ETHTransfer2View()
    {
        InitializeComponent();
    }
}