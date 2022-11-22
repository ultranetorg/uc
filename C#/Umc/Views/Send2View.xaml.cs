namespace UC.Umc.Views;

public partial class Send2View : ContentView
{
    public static readonly BindableProperty SourceAccountProperty =
		BindableProperty.Create(nameof(SourceAccount), typeof(AccountViewModel), typeof(Send1View));

    public static readonly BindableProperty RecipientAccountProperty =
		BindableProperty.Create(nameof(RecipientAccount), typeof(AccountViewModel), typeof(Send1View));

    public static readonly BindableProperty AmountProperty =
		BindableProperty.Create(nameof(Amount), typeof(decimal), typeof(Send1View));

    public static readonly BindableProperty ComissionProperty =
		BindableProperty.Create(nameof(Comission), typeof(decimal), typeof(Send1View));

    public AccountViewModel SourceAccount
    {
        get { return (AccountViewModel)GetValue(SourceAccountProperty); }
        set { SetValue(SourceAccountProperty, value); }
    }

    public AccountViewModel RecipientAccount
    {
        get { return (AccountViewModel)GetValue(RecipientAccountProperty); }
        set { SetValue(RecipientAccountProperty, value); }
    }

    public decimal Amount
    {
        get { return (decimal)GetValue(AmountProperty); }
        set { SetValue(AmountProperty, value); }
    }

    public decimal Comission
    {
        get { return (decimal)GetValue(ComissionProperty); }
        set { SetValue(ComissionProperty, value); }
    }

    public Send2View()
    {
        InitializeComponent();
    }
}
