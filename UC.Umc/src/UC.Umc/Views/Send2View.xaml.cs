using UC.Umc.Models;

namespace UC.Umc.Views;

public partial class Send2View : ContentView
{
	public static readonly BindableProperty SourceAccountProperty =
		BindableProperty.Create(nameof(SourceAccount), typeof(AccountModel), typeof(Send1View));

	public static readonly BindableProperty RecipientAccountProperty =
		BindableProperty.Create(nameof(RecipientAccount), typeof(AccountModel), typeof(Send1View));

	public static readonly BindableProperty AmountProperty =
		BindableProperty.Create(nameof(Amount), typeof(string), typeof(Send1View));

	public static readonly BindableProperty ComissionProperty =
		BindableProperty.Create(nameof(Comission), typeof(string), typeof(Send1View));

	public AccountModel SourceAccount
	{
		get => (AccountModel) GetValue(SourceAccountProperty);
		set => SetValue(SourceAccountProperty, value);
	}

	public AccountModel RecipientAccount
	{
		get => (AccountModel) GetValue(RecipientAccountProperty);
		set => SetValue(RecipientAccountProperty, value);
	}

	public string Amount
	{
		get => (string) GetValue(AmountProperty);
		set => SetValue(AmountProperty, value);
	}

	public string Comission
	{
		get => (string) GetValue(ComissionProperty);
		set => SetValue(ComissionProperty, value);
	}

	public Send2View()
	{
		InitializeComponent();
	}
}
