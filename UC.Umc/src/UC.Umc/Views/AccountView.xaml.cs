using UC.Umc.Models;

namespace UC.Umc.Views;

public partial class AccountView : ContentView
{
	public static readonly BindableProperty AccountProperty =
		BindableProperty.Create(nameof(Account), typeof(AccountModel), typeof(AccountView));

	public static readonly BindableProperty BorderStyleProperty =
		BindableProperty.Create(nameof(BorderStyle), typeof(Style), typeof(AccountView));

	public AccountModel Account
	{
		get => (AccountModel) GetValue(AccountProperty);
		set => SetValue(AccountProperty, value);
	}

	public Style BorderStyle
	{
		get => (Style) GetValue(BorderStyleProperty);
		set => SetValue(BorderStyleProperty, value);
	}

	public AccountView()
	{
		InitializeComponent();
	}
}